using System.Linq;
using System.Windows.Controls;
using wpf_projekt.models;
using wpf_projekt.Models;

namespace wpf_projekt.Views
{
    public partial class EventLogsView : UserControl
    {
        public EventLogsView()
        {
            InitializeComponent();

            using var context = new AppDbContext();

            LogsGrid.ItemsSource = context.EventLogs
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }
}