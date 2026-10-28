using OliveBranchBakes.Forms;
using System.Drawing;

namespace OliveBranchBakes.Navigation;

internal sealed class BakeryMainForm : Form
{
    private Form? _activePage;
    internal static BakeryMainForm? Current { get; private set; }

    public BakeryMainForm()
    {
        Current = this;
        Text = "Olive Branch Bakes | v13";
        ClientSize = new Size(1140, 760);
        MinimumSize = new Size(970, 680);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(251, 247, 237);
        ShowPage(Page.Welcome);
    }

    internal void ShowPage(Page destination)
    {
        Form next = destination switch
        {
            Page.Welcome => new WelcomeForm(),
            Page.CustomerOrder => new CustomerOrderForm(),
            Page.CustomerAccount => new CustomerAccountForm(),
            Page.OrderSummary => new OrderSummaryForm(),
            Page.AdministratorLogin => new AdministratorLoginForm(),
            Page.AdministratorConsole => new AdministratorConsoleForm(),
            Page.SavedData => new SavedDataForm(),
            _ => throw new ArgumentOutOfRangeException(nameof(destination))
        };
        // Embed the existing page inside the main window; no extra top-level window.
        next.TopLevel = false;
        next.FormBorderStyle = FormBorderStyle.None;
        next.ShowInTaskbar = false;
        next.StartPosition = FormStartPosition.Manual;
        next.MinimumSize = Size.Empty;
        next.MaximumSize = Size.Empty;
        next.AutoScaleMode = AutoScaleMode.None;
        next.Dock = DockStyle.Fill;

        // Initialize the page's responsive layout using the original design coordinates.
        if (destination != Page.SavedData)
            ResponsivePageLayout.Attach(next);

        var old = _activePage;
        SuspendLayout();
        if (old is not null)
        {
            old.Hide();
            Controls.Remove(old);
        }
        _activePage = next;
        Controls.Add(next);
        next.Show();
        next.BringToFront();
        Text = "Olive Branch Bakes | v13 - " + destination;
        ResumeLayout(true);

        // Dispose after the event handler that initiated navigation has returned.
        if (old is not null)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(old.Dispose));
            else
                old.Dispose();
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Current = null;
        base.OnFormClosed(e);
    }
}
