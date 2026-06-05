using NLog;
using System.Text;
using System.Text.Json;

namespace Team1_Pair_Program;

public class Program
{
    private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private static readonly List<Product> products =
        [
            new() { ProductName = "ノートPC", ProductPrice = 1500 },
            new() { ProductName = "マウス", ProductPrice = 5800 },
            new() { ProductName = "キーボード", ProductPrice = 12500 }
        ];
    private static readonly HttpClient httpClient = new HttpClient();

    public static List<Product> cart = [];
    static async Task Main()
    {
        var cart = new List<Product>();

        while (true)
        {
            string[] mainOptions = { "レジシステム表示", "履歴表示", "アプリ終了" };
            int mainSelectedIndex = 0;
            ConsoleKey key = ConsoleKey.NoName;

            while (key != ConsoleKey.Enter)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine("売上システム選択画面");
                Console.WriteLine("======================================");
                Console.WriteLine("矢印キー [↑][↓] で移動、[Enter] で決定\n");

                (mainSelectedIndex, key) = RadioMenu(mainOptions, mainSelectedIndex, key);
            }

            if (mainSelectedIndex == 2) break;

            if (mainSelectedIndex == 1)
            {
                Console.Clear();
                //if (orderHistory.Count == 0)
                //{
                //    Console.WriteLine("履歴はありません。\n何かキーを押すとメインメニューに戻ります。");
                //    Console.ReadKey(true);
                //    continue;
                //}
                string[] sortOptions = { "合計金額の降順 (低い順)", "日付順 (新しい順)" };
                int sortSelectedIndex = 0;
                key = ConsoleKey.NoName;

                while (key != ConsoleKey.Enter)
                {
                    Console.Clear();
                    Console.WriteLine("表示する履歴の並び順を選択してください");
                    Console.WriteLine("矢印キー [↑][↓] で移動、[Enter] で決定\n");
                    (sortSelectedIndex, key) = RadioMenu(sortOptions, sortSelectedIndex, key);
                }


                // sortSelectedIndex が 0 なら金額降順(Order)、1 なら日付降順(OrderByDescending)
                //var sortedHistory = (sortSelectedIndex == 0)
                //    ? orderHistory.OrderBy(o => o.TotalCost).ToList()
                //    : orderHistory.OrderByDescending(o => o.PurchaseDate).ToList();

                //並び替えた履歴の表示
                Console.Clear();
                Console.WriteLine($"購入履歴一覧 【並び替え: {sortOptions[sortSelectedIndex]}】");
                Console.WriteLine("--------------------------------------------");

                //foreach (var order in sortedHistory)
                //{
                //    // 日付は yyyy/MM/dd HH:mm:ss 形式で表示
                //    Console.WriteLine($" 商品名: {order.ProductName}");
                //    Console.WriteLine($" 個数: {order.ProductQuantity}");
                //    Console.WriteLine($"日付: {order.PurchaseDate:yyyy/MM/ddHH:mm:ss}");
                //    Console.WriteLine($"合計金額: {order.TotalCost:N0}円");
                //    Console.WriteLine("--------------------------------------------");
                //}

                Console.WriteLine("\n何かキーを押すとメインメニューに戻ります。");
                Console.ReadKey(true);
                continue;
            }

            while (true)
            {
                int selectedNameIndex = 0;
                key = ConsoleKey.NoName;
                string[] productMenus = products.Select(p => $"{p.ProductName} ({p.ProductPrice:N0}円)").ToArray();
                while (key != ConsoleKey.Enter)
                {
                    Console.Clear();
                    Console.WriteLine(@"商品を選択してください");
                    (selectedNameIndex, key) = RadioMenu(productMenus, selectedNameIndex, key);
                }

                Product selectedProduct = products[selectedNameIndex];

                int selectedQuantity = 0;
                Console.WriteLine("\n購入する数量を入力してください (1以上) ");
                Console.WriteLine($"選択した商品: {selectedProduct.ProductName}");
                string input = Console.ReadLine();

                // 1以上の正しい数値が入力されたかチェック
                if (!int.TryParse(input, out int result) || result <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n【エラー】0以下の数値や文字は入力できません。商品選択に戻ります。");
                    Console.ResetColor();
                    await Task.Delay(1000);
                    continue;
                }
                selectedQuantity = result;

                string[] actionOptions = { "追加", "キャンセル" };
                int actionIndex = 0;
                key = ConsoleKey.NoName;
                while (key != ConsoleKey.Enter)
                {
                    Console.Clear();
                    Console.WriteLine("商品確認");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine($" 商品名 : {selectedProduct.ProductName}");
                    Console.WriteLine($" 数量   : {selectedQuantity} 個");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("上記の内容を追加しますか？\n");

                    (actionIndex, key) = RadioMenu(actionOptions, actionIndex, key);
                }

                if (actionIndex == 1)
                {
                    Console.WriteLine("キャンセルしました。商品選択に戻ります。");
                    await Task.Delay(1000);
                    continue;
                }
                int existingIndex = cart.FindIndex(item => item.ProductName == selectedProduct.ProductName);
                if (existingIndex >= 0) cart[existingIndex].ProductQuantity += selectedQuantity;
                else
                {
                    cart.Add(new Product
                    {
                        ProductName = selectedProduct.ProductName,
                        ProductPrice = selectedProduct.ProductPrice,
                        ProductQuantity = selectedQuantity
                    });
                }

                string[] nextOptions = { "確定", "買い物を続ける" };
                int nextIndex = 0;
                key = ConsoleKey.NoName;
                while (key != ConsoleKey.Enter)
                {
                    Console.Clear();
                    Console.WriteLine("購入確認");
                    Console.WriteLine("--------------------------------------------");
                    foreach (var item in cart)
                    {
                        Console.WriteLine($"商品名: {item.ProductName}");
                        Console.WriteLine($"個数: {item.ProductQuantity}個\n");
                    }
                    Console.WriteLine("--------------------------------------------");

                    (nextIndex, key) = RadioMenu(nextOptions, nextIndex, key);
                }

                if (nextIndex == 1) continue;
                else if (nextIndex == 0)
                {
                    foreach (var item in cart)
                    {
                        item.PurchaseDate = DateTime.Now;
                    }
                    Console.Clear();
                    Console.WriteLine("購入商品一覧");
                    Console.WriteLine("--------------------------------------------");
                    foreach (var item in cart)
                    {
                        Console.WriteLine($"商品名: {item.ProductName}");
                        Console.WriteLine($"個数: {item.ProductQuantity}個\n");
                    }
                    Console.WriteLine("--------------------------------------------");

                    foreach (var item in cart)
                    {
                        item.TotalCost = item.ProductPrice * item.ProductQuantity;
                        // 送信用データモデルの作成
                        var orderData = new
                        {
                            ProductName = item.ProductName,
                            TotalCost = item.TotalCost,
                            Quantity = item.ProductQuantity,
                            PurchaseDate = item.PurchaseDate
                        };
                        string json = JsonSerializer.Serialize(orderData);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        logger.Info("サーバーへデータをPOST送信中...");
                        try
                        {
                            var response = await httpClient.PostAsync("http://localhost:8080/", content);
                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();
                                logger.Info($"通信成功: サーバーからのレスポンス:{responseBody}");
                            }
                            else
                            {
                                logger.Error($"サーバーがエラーを返しました: {(int)response.StatusCode} {response.StatusCode}");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"送信エラー: {item.ProductName} の送信に失敗: {ex.Message}");
                        }
                    }
                    cart.Clear();

                    logger.Info("売上データをPOST送信しました。");
                    Console.WriteLine("--------------------------------------------");
                    Console.WriteLine("何かキーを押すとメインメニューに戻ります。");
                    Console.ReadKey(true);
                    break;

                }
            }

        }
    }

    static (int index, ConsoleKey key) RadioMenu(string[] options, int selectedIndex, ConsoleKey key)
    {

        for (int i = 0; i < options.Length; i++)
        {
            Console.Write((i == selectedIndex) ? " (*) " : " ( ) ");
            Console.WriteLine(options[i]);
        }

        key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.UpArrow && selectedIndex > 0) selectedIndex--;
        else if (key == ConsoleKey.DownArrow && selectedIndex < options.Length - 1) selectedIndex++;

        return (selectedIndex, key);
    }
}
