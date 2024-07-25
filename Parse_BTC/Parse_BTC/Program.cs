using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json.Linq;

namespace Parse
{
    class Program
    {
        private static readonly HttpClient Client = new HttpClient();

        static async Task Main(string[] args)
        {
            try
            {
                string apiUrl = "https://api.blockcypher.com/v1/btc/main";
                var response = await Client.GetStringAsync(apiUrl);
                var blockchainInfo = JObject.Parse(response);
                var latestBlockUrl = blockchainInfo["latest_url"]?.ToString();

                if (string.IsNullOrEmpty(latestBlockUrl))
                {
                    Console.WriteLine("Не удалось получить URL последнего блока.");
                    return;
                }

                var blockResponse = await Client.GetStringAsync(latestBlockUrl);
                var block = JObject.Parse(blockResponse);

                var blockData = new BlockData
                {
                    Hash = block["hash"]?.ToString() ?? "N/A",
                    Height = block["height"]?.ToString() ?? "N/A",
                    Time = block["time"]?.ToString() ?? "N/A",
                    PrevBlock = block["prev_block"]?.ToString() ?? "N/A",
                    MerkleRoot = block["mrkl_root"]?.ToString() ?? "N/A",
                    TxCount = block["n_tx"]?.ToString() ?? "N/A"
                };

                var transactions = new List<Transaction>();
                foreach (var txid in block["txids"])
                {
                    transactions.Add(new Transaction
                    {
                        TxHash = txid.ToString()
                    });
                }

                await using (var writer = new StreamWriter("block_data.csv"))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecord(blockData);
                    await csv.NextRecordAsync();
                    await csv.WriteRecordsAsync(transactions);
                }

                Console.WriteLine("Данные блока сохранены в block_data.csv");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Ошибка при запросе данных: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Произошла ошибка: {e.Message}");
            }
        }
    }

    public class BlockData
    {
        public string Hash { get; set; }
        public string Height { get; set; }
        public string Time { get; set; }
        public string PrevBlock { get; set; }
        public string MerkleRoot { get; set; }
        public string TxCount { get; set; }
    }

    public class Transaction
    {
        public string TxHash { get; set; }
    }
}
