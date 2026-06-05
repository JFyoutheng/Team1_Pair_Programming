namespace Team1_Pair_Program;

public class Product
{
    public string ProductName { get; set; }
    public int TotalCost { get; set; }
    //クライアント側のみ必要
    public int ProductPrice { get; set; }
    public int ProductQuantity { get; set; }
    public DateTime PurchaseDate { get; set; }
}
