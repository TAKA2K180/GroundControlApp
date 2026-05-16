using GroundControlApp.Main.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GroundControlApp.Main.Models;

public sealed class CartItem : ObservableObject
{
    private int quantity;
    private string addOnNameInput = string.Empty;
    private string addOnPriceInput = string.Empty;

    public event EventHandler? TotalChanged;

    public CartItem(string name, string note, int quantity, decimal unitPrice)
    {
        Name = name;
        Note = note;
        this.quantity = quantity;
        UnitPrice = unitPrice;
        AddOns.CollectionChanged += OnAddOnsChanged;
        RecalculateTotals();
    }

    public string Name { get; }

    public string Note { get; }

    public int Quantity
    {
        get => quantity;
        set
        {
            if (SetProperty(ref quantity, Math.Max(1, value)))
            {
                OnPropertyChanged(nameof(Detail));
                RecalculateTotals();
            }
        }
    }

    public decimal UnitPrice { get; }

    public ObservableCollection<CartAddOn> AddOns { get; } = [];

    public decimal AddOnTotal => AddOns.Sum(addOn => addOn.Price);

    public decimal BaseTotal => Quantity * UnitPrice;

    public decimal Total => Quantity * (UnitPrice + AddOnTotal);

    public string BaseTotalDisplay => $"PHP {BaseTotal:N2}";

    public string TotalDisplay => $"PHP {Total:N2}";

    public string Detail => $"Qty {Quantity}{(string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}")}";

    public string AddOnNameInput
    {
        get => addOnNameInput;
        set => SetProperty(ref addOnNameInput, value);
    }

    public string AddOnPriceInput
    {
        get => addOnPriceInput;
        set => SetProperty(ref addOnPriceInput, value);
    }

    public void AddAddOn(string name, decimal price)
    {
        AddOns.Add(new CartAddOn(name, price));
        AddOnNameInput = string.Empty;
        AddOnPriceInput = string.Empty;
        RefreshTotals();
    }

    public void RemoveAddOn(CartAddOn addOn)
    {
        AddOns.Remove(addOn);
        RefreshTotals();
    }

    private void RefreshTotals()
    {
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(AddOnTotal));
        OnPropertyChanged(nameof(BaseTotal));
        OnPropertyChanged(nameof(BaseTotalDisplay));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(TotalDisplay));
        TotalChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnAddOnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshTotals();
    }
}
