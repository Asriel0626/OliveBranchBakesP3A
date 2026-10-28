using OliveBranchBakes.Navigation;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using OliveBranchBakes.Data;

namespace OliveBranchBakes.Forms;

/// <summary>Order summary screen.</summary>
public partial class OrderSummaryForm : Form
{
    public OrderSummaryForm()
    {
        InitializeComponent();
        LoadPreviewImages();
        ShowSavedOrdersPreview();
    }

    private void ShowSavedOrdersPreview()
    {
        var saved = AppData.Store.Orders;
        var recent = saved.OrderByDescending(x => x.PlacedAt).Take(2).ToList();
        nextTitle.Text = "Saved sample orders";
        pageSubtitle.Text = "The shopping cart is not active yet. Saved test orders are shown on the right.";
        emptyDetail.Text = "Cart and checkout will be added later.\nSaved sample orders are available in the panel on the right.";
        nextDescription.Text = $"{saved.Count} orders saved locally\n\n" +
            string.Join("\n", recent.Select(order =>
                $"{order.ConfirmationNumber}  |  {DisplayFormat.Usd(order.Total)}")) +
            "\n\nThese are test records, not a shopping cart.";
        summaryAccount.Text = "VIEW SAVED DATA";
        summaryAccount.Tag = "SavedData";
        placeDisclaimer.Text = "Checkout is not active. Use Saved Data to review sample orders.";
    }

    private void Navigate_Click(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Tag is string target &&
            Enum.TryParse<Page>(target, out Page page))
        {
            BakeryApplicationContext.Navigate(page);
        }
    }

    private void PreviewOnly_Click(object? sender, EventArgs e)
    {
        EnglishDialog.Show(
            "This feature is not available yet.",
            "Olive Branch Bakes",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ProjectInfo_Click(object? sender, EventArgs e)
    {
        EnglishDialog.Show(
            "Project: Olive Branch Bakes\nStudent: EnYan Chen\nCourse: CS3388 Object-Oriented Programming\n" +
            "Application: menu and order record storage\n" +
            "Saved records are available. Account and checkout features are not implemented yet.",
            "About the project", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LoadPreviewImages()
    {
        // No images on this page.
    }
}
