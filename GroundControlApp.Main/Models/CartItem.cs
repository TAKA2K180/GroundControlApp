using GroundControlApp.Main.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GroundControlApp.Main.Models;

public sealed class CartItem : ObservableObject
{
    private int quantity;
    private decimal discountAmount;

    public event EventHandler? TotalChanged;

    public CartItem(Guid? menuId, string name, int categoryId, string category, string note, int quantity, decimal unitPrice)
    {
        MenuId = menuId;
        Name = name;
        CategoryId = categoryId;
        Category = category;
        Note = note;
        this.quantity = quantity;
        UnitPrice = unitPrice;
        AddOns.CollectionChanged += OnAddOnsChanged;
        RecalculateTotals();
    }

    public Guid? MenuId { get; }

    public string Name { get; }

    public int CategoryId { get; }

    public string Category { get; }

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

    public decimal GrossTotal => Quantity * (UnitPrice + AddOnTotal);

    public decimal DiscountAmount
    {
        get => discountAmount;
        private set
        {
            if (SetProperty(ref discountAmount, Math.Clamp(value, 0, GrossTotal)))
            {
                OnPropertyChanged(nameof(DiscountDisplay));
                RecalculateTotals();
            }
        }
    }

    public decimal BaseTotal => Quantity * UnitPrice;

    public decimal Total => Math.Max(0, GrossTotal - DiscountAmount);

    public string BaseTotalDisplay => $"PHP {BaseTotal:N2}";

    public string TotalDisplay => $"PHP {Total:N2}";

    public string DiscountDisplay => DiscountAmount <= 0 ? "No discount" : $"- PHP {DiscountAmount:N2}";

    public string Detail => $"Qty {Quantity}{(string.IsNullOrWhiteSpace(Note) ? string.Empty : $" - {Note}")}";

    public void ToggleDiscount()
    {
        DiscountAmount = DiscountAmount > 0 ? 0 : Math.Round(GrossTotal * 0.10m, 2);
    }

    public void AddAddOn(Guid? addOnId, string name, decimal price)
    {
        AddOns.Add(new CartAddOn(addOnId, name, price));
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
        OnPropertyChanged(nameof(GrossTotal));
        if (DiscountAmount > GrossTotal)
        {
            discountAmount = GrossTotal;
            OnPropertyChanged(nameof(DiscountAmount));
            OnPropertyChanged(nameof(DiscountDisplay));
        }
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
