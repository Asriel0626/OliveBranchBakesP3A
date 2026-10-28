using OliveBranchBakes.Data;
using OliveBranchBakes.Models;
using OliveBranchBakes.Navigation;
using OliveBranchBakes.Services;
using System.ComponentModel;
using System.Drawing;

namespace OliveBranchBakes.Forms;

/// <summary>Displays products from the saved menu, filtered by category.</summary>
public partial class CustomerOrderForm : Form
{
    private CatalogService? _catalog;
    private Panel? _scrollableMenu;
    private bool _resizingMenu;
    private bool _fittingCards;

    private sealed class MenuFilterOption
    {
        public string Label { get; }
        public string? CategoryId { get; }
        public bool IsSeasonal { get; }

        public MenuFilterOption(string label, string? categoryId = null, bool isSeasonal = false)
        {
            Label = label;
            CategoryId = categoryId;
            IsSeasonal = isSeasonal;
        }

        public override string ToString() => Label;
    }

    public CustomerOrderForm()
    {
        InitializeComponent();
        // Replace the designer's sample cards only when the application runs.
        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            LoadPreviewImages();
            return;
        }

        SetupLiveCatalog();
    }

    private void SetupLiveCatalog()
    {
        try
        {
            _catalog = new CatalogService(AppData.Store);

            catalogCard.SuspendLayout();
            foreach (Panel sample in new[] { product0, product1, product2, product3 })
            {
                catalogCard.Controls.Remove(sample);
                sample.Dispose();
            }

            _scrollableMenu = new Panel
            {
                Name = "jsonMenuList",
                Location = new Point(15, 62),
                Size = new Size(654, 387),
                AutoScroll = true,
                BackColor = Color.FromArgb(255, 253, 249),
                TabIndex = 6
            };
            catalogCard.Controls.Add(_scrollableMenu);
            catalogCard.SizeChanged += (_, _) => UpdateMenuLayout();
            _scrollableMenu.SizeChanged += (_, _) => FitProductCards();
            catalogCard.ResumeLayout(true);
            UpdateMenuLayout();

            pageSubtitle.Text = "Browse our baked goods by category. Online ordering is coming soon.";
            basketEmpty.Text = "Your basket is empty.\nCart functionality is not available yet.";
            menuBadge.Text = "BAKERY MENU";

            categoryFilter.Items.Clear();
            categoryFilter.Items.Add(new MenuFilterOption("All baked goods"));
            foreach (BakeryCategory category in _catalog.Categories())
                categoryFilter.Items.Add(new MenuFilterOption(category.Name, category.Id));
            categoryFilter.Items.Add(new MenuFilterOption("Seasonal", isSeasonal: true));

            categoryFilter.SelectedIndexChanged += CategoryFilter_SelectedIndexChanged;
            categoryFilter.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            categoryFilter.Enabled = false;
            menuBadge.Text = "MENU UNAVAILABLE";
            catalogHeading.Text = "Unable to load saved menu";
            EnglishDialog.Show(this,
                "The saved bakery menu could not be loaded.\n\n" + ex.Message +
                "\n\nCheck the JSON files in the Phase3A data folder.",
                "Menu load error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CategoryFilter_SelectedIndexChanged(object? sender, EventArgs e) => RenderFilteredProducts();

    private void RenderFilteredProducts()
    {
        if (_catalog is null || _scrollableMenu is null) return;
        var filter = categoryFilter.SelectedItem as MenuFilterOption;
        if (filter is null) return;

        IReadOnlyList<BakeryProduct> products = _catalog.AvailableOn(DateTime.Today, filter.CategoryId);
        if (filter.IsSeasonal)
            products = products.Where(p => p.IsSeasonal).ToList();

        catalogHeading.Text = filter.CategoryId is null && !filter.IsSeasonal
            ? "Today's bakery menu"
            : filter.Label + " selection";
        menuBadge.Text = $"{products.Count} AVAILABLE";

        _scrollableMenu.SuspendLayout();
        // Release images and controls from the previous category.
        foreach (Control oldCard in _scrollableMenu.Controls.Cast<Control>().ToArray())
        {
            _scrollableMenu.Controls.Remove(oldCard);
            oldCard.Dispose();
        }
        _scrollableMenu.AutoScrollPosition = Point.Empty;

        if (products.Count == 0)
        {
            Label empty = new()
            {
                Location = new Point(12, 18),
                Size = new Size(592, 120),
                Text = filter.IsSeasonal
                    ? "No seasonal items are available today. Seasonal products appear here only during their listed dates."
                    : "No products are available in this category today. Try another category.",
                ForeColor = Color.FromArgb(105, 75, 54),
                Font = new Font("Segoe UI", 11f),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _scrollableMenu.Controls.Add(empty);
            _scrollableMenu.AutoScrollMinSize = new Size(0, 150);
        }
        else
        {
            for (int i = 0; i < products.Count; i++)
                _scrollableMenu.Controls.Add(CreateProductCard(products[i], i * 98));
            _scrollableMenu.AutoScrollMinSize = new Size(0, products.Count * 98);
        }

        _scrollableMenu.ResumeLayout(true);
        UpdateMenuLayout();
    }

    // The catalog frame is resized by the page layout. Keep its product list inside
    // that frame instead of leaving the list at its original design-time size.
    internal void UpdateMenuLayout()
    {
        if (_resizingMenu || _scrollableMenu is null || _scrollableMenu.IsDisposed)
            return;

        _resizingMenu = true;
        try
        {
            int margin = Math.Max(12, (int)Math.Round(catalogCard.ClientSize.Width * 15f / 688f));
            int top = Math.Max(61, catalogHeading.Bottom + 7);
            int availableWidth = Math.Max(200, catalogCard.ClientSize.Width - 2 * margin);
            int availableHeight = Math.Max(140, catalogCard.ClientSize.Height - top - margin);
            _scrollableMenu.SetBounds(margin, top, availableWidth, availableHeight);
            FitProductCards();
        }
        finally
        {
            _resizingMenu = false;
        }
    }

    private void FitProductCards()
    {
        if (_fittingCards || _scrollableMenu is null || _scrollableMenu.IsDisposed)
            return;

        _fittingCards = true;
        try
        {
            // Size the images and text using the actual menu viewport, rather than
            // the initial 654 x 387 placeholder used by the Phase 2 design.
            float widthRatio = _scrollableMenu.ClientSize.Width / 654f;
            float heightRatio = _scrollableMenu.ClientSize.Height / 387f;
            float rowScale = Math.Clamp(MathF.Sqrt(widthRatio * heightRatio), .85f, 1.75f);
            float textScale = Math.Clamp(rowScale, .9f, 1.6f);
            int width = Math.Max(200, _scrollableMenu.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4);
            int rowHeight = (int)Math.Round(91 * rowScale);
            int spacing = (int)Math.Round(98 * rowScale);
            int index = 0;
            _scrollableMenu.SuspendLayout();
            foreach (Control card in _scrollableMenu.Controls)
            {
                if (card is not Panel)
                {
                    card.Width = Math.Max(200, width - 12);
                    continue;
                }

                card.SuspendLayout();
                card.SetBounds(0, index++ * spacing, width, rowHeight);
                int image = (int)Math.Round(76 * rowScale);
                int imageX = (int)Math.Round(8 * rowScale);
                int imageY = (rowHeight - image) / 2;
                int left = imageX + image + (int)Math.Round(15 * rowScale);
                int buttonW = (int)Math.Round(86 * textScale);
                int buttonH = (int)Math.Round(48 * rowScale);
                int margin = Math.Max(10, (int)Math.Round(10 * textScale));
                int buttonX = width - buttonW - margin;
                int priceW = (int)Math.Round(90 * textScale);
                int priceX = buttonX - priceW - margin;
                int labelW = Math.Max(100, priceX - left - 8);
                foreach (Control part in card.Controls)
                {
                    switch (part.Name)
                    {
                        case "itemImage":
                            part.SetBounds(imageX, imageY, image, image); break;
                        case "itemName":
                            part.SetBounds(left, (int)(10 * rowScale), labelW, (int)(32 * rowScale)); break;
                        case "itemDescription":
                            part.SetBounds(left, (int)(43 * rowScale), labelW, (int)(28 * rowScale)); break;
                        case "itemPrice":
                            part.SetBounds(priceX, (int)(10 * rowScale), priceW, (int)(29 * rowScale)); break;
                        case "itemAdd":
                            part.SetBounds(buttonX, (rowHeight - buttonH) / 2, buttonW, buttonH); break;
                    }
                    if (part is Label or Button)
                    {
                        float desired = part.Name switch
                        {
                            "itemName" => 10.7f * textScale,
                            "itemDescription" => 9.2f * textScale,
                            "itemPrice" => 10.5f * textScale,
                            _ => 10f * textScale
                        };
                        if (Math.Abs(part.Font.SizeInPoints - desired) > .15f)
                        {
                            Font oldFont = part.Font;
                            part.Font = new Font("Segoe UI", desired, oldFont.Style);
                            oldFont.Dispose();
                        }
                    }
                }
                card.ResumeLayout(true);
            }
            _scrollableMenu.AutoScrollMinSize = new Size(0, index * spacing);
            _scrollableMenu.ResumeLayout(true);
        }
        finally
        {
            _fittingCards = false;
        }
    }

    private static Panel CreateProductCard(BakeryProduct product, int y)
    {
        Color olive = Color.FromArgb(55, 73, 44);
        Color brown = Color.FromArgb(105, 75, 54);
        Panel card = new()
        {
            Location = new Point(0, y),
            Size = new Size(627, 91),
            BackColor = Color.FromArgb(251, 247, 237)
        };

        string? imageName = product.Id switch
        {
            "BR001" => "sourdough.png",
            "BR002" => "brioche.png",
            "BR003" => "whole-wheat.png",
            "CO001" => "cookies.png",
            "CO002" => "oatmeal-cookie.png",
            "CO003" => "sugar-cookie.png",
            "MU001" => "blueberry-muffin.png",
            "MU002" => "lemon-poppy-muffin.png",
            "PA001" => "cinnamon.png",
            "PA002" => "croissant.png",
            "PI001" => "apple.png",
            "PI002" => "cherry-pie.png",
            "CA001" => "decorated-cake.png",
            "CA002" => "vanilla-cake.png",
            "EX001" => "honey-butter.png",
            "SEASON-0" => "cider-donut.png",
            "SEASON-1" => "pumpkin-bread.png",
            "SEASON-2" => "pecan-pie.png",
            "SEASON-3" => "sweet-potato-pie.png",
            "SEASON-4" => "gingerbread.png",
            "SEASON-5" => "cranberry-scone.png",
            "SEASON-6" => "peppermint-brownie.png",
            "SEASON-7" => "king-cake.png",
            "SEASON-8" => "sticky-bun.png",
            "SEASON-9" => "shortbread.png",
            _ => null
        };
        bool imageAdded = false;
        if (imageName is not null)
        {
            string imagePath = Path.Combine(AppContext.BaseDirectory, "Assets", imageName);
            if (File.Exists(imagePath))
            {
                try
                {
                    // Copy the image to avoid locking the file.
                    using Image source = Image.FromFile(imagePath);
                    Bitmap bitmap = new(source);
                    PictureBox picture = new()
                    {
                        Name = "itemImage",
                        Location = new Point(8, 7),
                        Size = new Size(78, 76),
                        BackColor = Color.FromArgb(255, 253, 249),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = bitmap
                    };
                    picture.Disposed += (_, _) => bitmap.Dispose();
                    card.Controls.Add(picture);
                    imageAdded = true;
                }
                catch (Exception) { }
            }
        }
        if (!imageAdded)
        {
            Label tile = new()
            {
                Name = "itemImage",
                Location = new Point(8, 7),
                Size = new Size(78, 76),
                BackColor = Color.FromArgb(232, 238, 219),
                ForeColor = olive,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Text = product.IsSeasonal ? "SEASON" : product.CategoryId.ToUpperInvariant() switch
                {
                    "BREADS" => "BREAD", "COOKIES" => "COOKIE", "MUFFINS" => "MUFFIN",
                    "PASTRIES" => "PASTRY", "PIES" => "PIE", "CAKES" => "CAKE",
                    "EXTRAS" => "EXTRA", _ => "BAKERY"
                },
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(tile);
        }

        card.Controls.Add(new Label
        {
            Name = "itemName",
            Location = new Point(101, 11),
            Size = new Size(321, 30),
            Text = product.Name,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 10.7f, FontStyle.Bold),
            ForeColor = olive,
            TextAlign = ContentAlignment.MiddleLeft
        });
        string description = product.IsSeasonal
            ? $"Seasonal: {product.SeasonStartMonth:00}/{product.SeasonStartDay:00} – {product.SeasonEndMonth:00}/{product.SeasonEndDay:00}"
            : product.Description;
        card.Controls.Add(new Label
        {
            Name = "itemDescription",
            Location = new Point(101, 43),
            Size = new Size(353, 28),
            Text = description,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 9.2f),
            ForeColor = brown,
            TextAlign = ContentAlignment.MiddleLeft
        });
        card.Controls.Add(new Label
        {
            Name = "itemPrice",
            Location = new Point(438, 10),
            Size = new Size(86, 27),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Text = DisplayFormat.Usd(product.Price),
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = brown,
            TextAlign = ContentAlignment.MiddleRight
        });
        Button add = new()
        {
            Name = "itemAdd",
            Location = new Point(531, 22),
            Size = new Size(86, 48),
            Text = "ADD +",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.FromArgb(223, 180, 81),
            ForeColor = olive,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand,
            TabStop = true
        };
        add.FlatAppearance.BorderSize = 0;
        add.Click += (_, _) => EnglishDialog.Show(
            "The shopping cart is not available yet.",
            "Olive Branch Bakes — Menu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        card.Controls.Add(add);
        return card;
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
            "The shopping cart and checkout are not available yet.",
            "Olive Branch Bakes — Menu",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ProjectInfo_Click(object? sender, EventArgs e)
    {
        EnglishDialog.Show(
            "Project: Olive Branch Bakes\nStudent: EnYan Chen\nCourse: CS3388 Object-Oriented Programming\n" +
            "Current features: product browsing and saved bakery data.\n" +
            "Cart and checkout will be implemented in the next checkpoints.",
            "About the project", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LoadPreviewImages()
    {
        PictureBox[] pictures = { picture0, picture1, picture2, picture3 };
        string[] names = { "sourdough.png", "cinnamon.png", "cookies.png", "apple.png" };
        for (int i = 0; i < pictures.Length; i++)
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Assets", names[i]);
                if (File.Exists(path)) pictures[i].Image = Image.FromFile(path);
            }
            catch (Exception) { }
        }
    }
}
