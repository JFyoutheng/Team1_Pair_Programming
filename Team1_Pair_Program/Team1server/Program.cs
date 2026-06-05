namespace Team1server
{
    using NLog;
    using System;
    using System.Collections.Generic; // 💡 Listを使うために必要
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Text.Json;

    class Program
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            List<Products> salesList = new List<Products>();

            // 1. サーバーの待ち受け設定
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://+:8080/");

            try
            {
                listener.Start();
            }
            catch (Exception)
            {
                // 管理者権限のエラーが出た場合は、localhostのみで試行します
                listener.Close();

                listener = new HttpListener();
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
                // 2. クライアントからの接続を待つ
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                try
                {
                    // 3. HTTPメソッドに応じた処理
                    if (request.HttpMethod == "POST")
                    {
                        // 送られてきた指示（JSON）を読み取る
                        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                        string jsonString = reader.ReadToEnd();

                        try
                        {
                            Products? data = JsonSerializer.Deserialize<Products>(jsonString);
                            if (data != null)
                            {
                                salesList.Add(data);
                                Console.WriteLine($"【変換成功】");
                                Console.WriteLine($"商品名: {data.ProductName}");
                                Console.WriteLine($"数量: {data.ProductQuantity}");
                                Console.WriteLine($"金額: {data.TotalCost}"); // 「年齢」から「金額」に修正しました
                            }
                        }
                        catch (JsonException)
                        {
                            Console.WriteLine("JSONの形式が正しくありません。");
                        }

                        Console.WriteLine($"[指示受信] 時刻: {DateTime.Now:HH:mm:ss}\n");

                        // 4. 返事（JSONデータ）を返す準備
                        string jsonResponse = "{\"status\": \"success\", \"message\": \"機器1の制御に成功しました。\"}";
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK; // 200 OK
                        response.ContentLength64 = buffer.Length;

                        // 💡 データを書き込む（Closeは response.Close() に任せる）
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("クライアントへ応答データを返却しました。\n");
                    }
                    else if (request.HttpMethod == "GET")
                    {
                        // 💡 GETリクエストが来た場合の処理（必要であればここに書く）
                        string jsonResponse = "{\"message\": \"GETリクエストを受け付けました。\"}";
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                        response.StatusCode = (int)HttpStatusCode.OK;
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        // POSTでもGETでもない場合
                        response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"リクエスト処理中にエラー: {ex.Message}");
                }
                finally
                {
                    // 💡 【超重要】どんなリクエスト（POST, GET, エラー）であっても、
                    // 最後に必ず「この通信(response)」だけを確実に閉じる！
                    response.Close();
                }
            }
        }

        public class Products
        {
            public string ProductName { get; set; } = "";
            public string ProductQuantity { get; set; } = "";
            public int TotalCost { get; set; }
            public DateTime PurchaseDate { get; set; }
        }
    }
}