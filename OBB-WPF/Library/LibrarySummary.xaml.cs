using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OBB_WPF.Library
{
    /// <summary>
    /// Interaction logic for LibrarySummary.xaml
    /// </summary>
    public partial class LibrarySummary : Window
    {
        public LibrarySummary(List<Series> seriesList)
        {
            InitializeComponent();

            var sb = new StringBuilder();

            var fullyEdited = seriesList.Where(x => x.Volumes.All(y => y.EditedBy.Any(z => !string.IsNullOrWhiteSpace(z)) || DateTime.Parse(y.Published!) > DateTime.UtcNow.Date)
                && x.Volumes.Any(y => y.EditedBy.Any(z => !string.IsNullOrWhiteSpace(z)))).ToList();

            seriesList.RemoveAll(x => fullyEdited.Contains(x));

            sb.AppendLine($"Omnibus Builder UI Series Support (Last updated {DateTime.UtcNow.Date:dd MMMM yyyy})");
            if (fullyEdited.Count > 0)
            {
                sb.AppendLine($"Fully Edited Series");
                sb.AppendLine("-");
                sb.AppendLine("|Series|Published Volumes|Unpublished Volumes|Editor");
                sb.AppendLine("|-|-|-|-|");

                foreach(var series in fullyEdited)
                {
                    sb.Append($"|{series.Name}|{series.Volumes.Count(x => DateTime.Parse(x.Published!) <= DateTime.UtcNow.Date)}|{series.Volumes.Count(x => DateTime.Parse(x.Published!) > DateTime.UtcNow.Date)}|");
                    bool first = true;
                    foreach(var editor in series.Volumes.SelectMany(x => x.EditedBy).Distinct())
                    {
                        if (!first)
                        {
                            sb.Append(", ");
                        }
                        else
                        {
                            first = false;
                        }
                        sb.Append(editor);
                    }

                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            var partEdited = seriesList.Where(x => x.Volumes.Any(y => y.EditedBy.Any(z => !string.IsNullOrWhiteSpace(z)))).ToList();
            seriesList.RemoveAll(x => partEdited.Contains(x));

            if (partEdited.Count > 0)
            {
                sb.AppendLine("Partially Edited Series");
                sb.AppendLine("-");
                sb.AppendLine("|Series|Published Volumes|Unpublished Volumes|Editor");
                sb.AppendLine("|-|-|-|-|");

                foreach (var series in partEdited)
                {
                    sb.Append($"|{series.Name}|{series.Volumes.Count(x => DateTime.Parse(x.Published!) <= DateTime.UtcNow.Date)}|{series.Volumes.Count(x => DateTime.Parse(x.Published!) > DateTime.UtcNow.Date)}|");
                    bool first = true;
                    foreach (var editor in series.Volumes.SelectMany(x => x.EditedBy).Distinct())
                    {
                        if (!first)
                        {
                            sb.Append(", ");
                        }
                        else
                        {
                            first = false;
                        }
                        sb.Append(editor);
                    }

                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            if (seriesList.Count > 0)
            {
                sb.AppendLine("Auto-Generated Series");
                sb.AppendLine("-");
                sb.AppendLine("|Series|Published Volumes|Unpublished Volumes");
                sb.AppendLine("|-|-|-|");

                foreach (var series in seriesList)
                {
                    sb.AppendLine($"|{series.Name}|{series.Volumes.Count(x => DateTime.Parse(x.Published!) <= DateTime.UtcNow.Date)}|{series.Volumes.Count(x => DateTime.Parse(x.Published!) > DateTime.UtcNow.Date)}");
                }

                sb.AppendLine();
            }

            SummaryBox.Text = sb.ToString();
        }
    }
}
