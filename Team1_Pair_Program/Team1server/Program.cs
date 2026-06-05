namespace Team1server
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.Json;

    class Program
    {
        static void Main(string[] args)
        {
            List<Products> salesList = new List<Products>;
            // 1. サーバーの待ち受け設定（すべてのIPアドレスからの接続をポート 8080 で許可）
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://+:8080/");

            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                // 管理者権限のエラーが出た場合は、localhostのみで試行します
                listener.Prefixes.Clear();
                listener.Prefixes.Add("http://localhost:8080/");
                listener.Start();
            }

            Console.WriteLine("=========================================");
            Console.WriteLine("【模擬・機器1スタブ】が起動しました");
            Console.WriteLine("   ポート番号: 8080");
            Console.WriteLine("=========================================");
            Console.WriteLine("ペアのPC（クライアント）からの通信を待っています...\n");

            while (true)
            {
                // 2. クライアントからの指示（POST）を待つ
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "POST")
                {
                    // 3. 送られてきた指示（JSON）を読み取る
                    using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    string jsonString = reader.ReadToEnd();
                    try
                    {
                        Products? data = JsonSerializer.Deserialize<Products>(jsonString);
                        if (data != null)
                        {
                            salesList.Add(data);
                            // クラスの変数として扱えるようになります
                            Console.WriteLine($"【変換成功】");
                            Console.WriteLine($"商品名: {data.ProductName}");
                            Console.WriteLine($"数量: {data.ProductQuantity}");
                            Console.WriteLine($"年齢: {data.TotalCost}");
                        }
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("JSONの形式が正しくありません。");
                    }




                    Console.WriteLine($"[指示受信] 時刻: {DateTime.Now:HH:mm:ss}\n");


                    //Console.WriteLine($"[受信データ] {jsonString}");

                    // 4. 機器が動作したと見立てて、返事（JSONデータ）を 200 OK で返す
                    string jsonResponse = "{\"status\": \"success\", \"message\": \"機器1の制御に成功しました。\"}";
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                    response.ContentType = "application/json";
                    response.StatusCode = (int)HttpStatusCode.OK; // 200 OK
                    response.ContentLength64 = buffer.Length;

                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();

                    Console.WriteLine("クライアントへ応答データを返却しました。\n");
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    response.Close();
                }
                if (request.HttpMethod == "GET")
                {

                }


            }
        }



        public class Products
        {
            public string ProductName { get; set; }
            public string ProductQuantity { get; set; }
            public int TotalCost { get; set; }
            public DateTime PurchaseDate { get; set; }
        }
    }
}
