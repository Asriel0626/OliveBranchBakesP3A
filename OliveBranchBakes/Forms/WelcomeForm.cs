using OliveBranchBakes.Navigation;
using OliveBranchBakes.Data;
using System.ComponentModel;
using System.Drawing;

namespace OliveBranchBakes.Forms;

/// <summary>Displays the bakery's welcome page and navigation.</summary>
public partial class WelcomeForm : Form
{
    public WelcomeForm()
    {
        InitializeComponent();
        LoadPreviewImages();
        Button demo = new()
        {
            Name = "viewSavedDataButton", Text = "VIEW SAVED DATA",
            Location = new Point(742, 26), Size = new Size(340, 52),
            BackColor = Color.FromArgb(79, 98, 62), ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold), TabIndex = 4
        };
        demo.FlatAppearance.BorderSize = 0;
        demo.Click += (_, _) => BakeryApplicationContext.Navigate(Page.SavedData);
        body.Controls.Add(demo);
        demo.BringToFront();
        phaseNotice.Location = new Point(310, 596);
        phaseNotice.Size = new Size(790, 42);
        phaseNotice.Text = "Menu records and sample orders can be saved and loaded.";
        pageSubtitle.Text = "Browse baked goods, or open Saved Data to review stored records.";
        footerLeft.Text = "OLIVE BRANCH BAKES  •  PRE-ORDER SYSTEM";
        footerRight.Text = "Customer checkout is not available yet";
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
            "Phase 2: Solution Architecture and Interface Prototype\nData storage: product, customer, and sample order records\n" +
            "Account, cart and checkout features are still in development.",
            "About the project", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LoadPreviewImages()
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "bakery-hero.png");
            if (File.Exists(path))
                heroPicture.Image = Image.FromFile(path);
        }
        catch (Exception)
        {
            // Design-time tools may not have the copied Assets directory.
        }
    }
}
