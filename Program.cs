using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;

namespace ExemploAPI
{
    class Programa
    {
        private static readonly HttpClient cliente = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Digite o nome de um País para saber mais detalhes: ");
            string pais = Console.ReadLine();

            await ObterPais(pais);
        }
        static async Task ObterPais(string pais)
        {
            try
            {
                HttpResponseMessage resposta = await cliente.GetAsync($"https://restcountries.com/v3.1/name/{pais}");
                resposta.EnsureSuccessStatusCode();

                string corpoResposta = await resposta.Content.ReadAsStringAsync();

                JArray dadosPais = JArray.Parse(corpoResposta);
                var infoPais = dadosPais[0];

                string? nome = infoPais["name"]["common"].ToString();
                string? capital = infoPais["capital"]?[0]?.ToString() ?? "N/A";

                Console.WriteLine($"\nPaís: {nome}");
                Console.WriteLine($"Capital: {capital}");

                string stringConexao = "Server=localhost,1433;Database=Projeto;User Id=SA;Password=03122002Gabriel.;TrustServerCertificate=True;";
                using (SqlConnection conexao = new SqlConnection(stringConexao))
                {
                    await conexao.OpenAsync();

                    string queryInsercao = "INSERT INTO teste (Nome, capital) VALUES (@Nome, @capital)";
                    using (SqlCommand comando = new SqlCommand(queryInsercao, conexao))
                    {
                        comando.Parameters.AddWithValue("@Nome", nome);
                        comando.Parameters.AddWithValue("@capital", capital);

                        int linhasAfetadas = await comando.ExecuteNonQueryAsync();
                        Console.WriteLine($"{linhasAfetadas} linha(s) inserida(s) no banco de dados.");
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nErro na requisição da API!");
                Console.WriteLine($"Mensagem: {e.Message}");
            }
            catch (SqlException e)
            {
                Console.WriteLine("\nErro ao conectar ou inserir no banco de dados!");
                Console.WriteLine($"Mensagem: {e.Message}");
            }
        }
    }
}
