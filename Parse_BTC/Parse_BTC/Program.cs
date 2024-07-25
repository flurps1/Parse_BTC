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

            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(apiUrl);

            JObject blockchainInfo = JObject.Parse(response);

            // Получение URL последнего блока
            string latestBlockUrl = blockchainInfo["latest_url"].ToString();

            // Запрос информации о последнем блоке
            var blockResponse = await client.GetStringAsync(latestBlockUrl);
            JObject block = JObject.Parse(blockResponse);

            // Получение данных блока
            var blockData = new
            {
                Hash = block["hash"].ToString(),
                Height = block["height"].ToString(),
                Time = block["time"].ToString(),
                PrevBlock = block["prev_block"].ToString(),
                MerkleRoot = block["mrkl_root"].ToString(),
                TxCount = block["n_tx"].ToString()
            };

            // Вывод данных блока в консоль
            Console.WriteLine("Данные блока:");
            Console.WriteLine($"Хэш: {blockData.Hash}");
            Console.WriteLine($"Высота: {blockData.Height}");
            Console.WriteLine($"Время: {blockData.Time}");
            Console.WriteLine($"Предыдущий блок: {blockData.PrevBlock}");
            Console.WriteLine($"Корень Меркла: {blockData.MerkleRoot}");
            Console.WriteLine($"Количество транзакций: {blockData.TxCount}");
            Console.WriteLine();

            // Список транзакций
            var transactions = new List<dynamic>();
            foreach (var txid in block["txids"])
            {
                transactions.Add(new
                {
                    TxHash = txid.ToString()
                });
            }

            // Вывод транзакций в консоль
            Console.WriteLine("Транзакции:");
            foreach (var tx in transactions)
            {
                Console.WriteLine($"TxHash: {tx.TxHash}");
            }

            // Сохранение данных в CSV
            using (var writer = new StreamWriter("block_data.csv"))
            using (var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Записываем данные блока
                csv.WriteRecord(blockData);
                csv.NextRecord();
                
                // Записываем заголовки транзакций
                csv.WriteHeader<dynamic>();
                csv.NextRecord();
                
                // Записываем транзакции
                csv.WriteRecords(transactions);
            }
            
            Console.WriteLine("Данные блока сохранены в block_data.csv");
        }
    }
}
