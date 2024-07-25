using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json.Linq;

namespace parse
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string apiUrl = "https://api.blockcypher.com/v1/btc/main";

            var client = new HttpClient();
            var response = await client.GetStringAsync(apiUrl);

            var blockchainInfo = JObject.Parse(response);

            var latestBlockUrl = blockchainInfo["latest_url"]?.ToString();

            var blockResponse = await client.GetStringAsync(latestBlockUrl);
            var block = JObject.Parse(blockResponse);

            var blockData = new
            {
                Hash = block["hash"]?.ToString() ?? "N/A",
                Height = block["height"]?.ToString() ?? "N/A",
                Time = block["time"]?.ToString() ?? "N/A",
                PrevBlock = block["prev_block"]?.ToString() ?? "N/A",
                MerkleRoot = block["mrkl_root"]?.ToString() ?? "N/A",
                TxCount = block["n_tx"]?.ToString() ?? "N/A"            };

            var transactions = new List<dynamic>();
            foreach (var txid in block["txids"])
            {
                transactions.Add(new
                {
                    TxHash = txid.ToString()
                });
            }

            await using (var writer = new StreamWriter("block_data.csv"))
            await using (var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecord(blockData);
                await csv.NextRecordAsync();
                
                csv.WriteHeader<dynamic>();
                await csv.NextRecordAsync();
                
                await csv.WriteRecordsAsync(transactions);
            }
            
            Console.WriteLine("Данные блока сохранены в block_data.csv");
        }
    }
}
