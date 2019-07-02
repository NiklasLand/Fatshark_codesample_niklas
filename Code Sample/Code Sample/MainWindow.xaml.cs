using MarkEmbling.PostcodesIO;
using MarkEmbling.PostcodesIO.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GMap;
using GMap.NET.WindowsPresentation;
using GMap.NET;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Code_Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _numberOfDomainsShown = 3;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void GetMostCommonDomains()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<KeyValuePair<string, int>> result = DataBaseInterface.GetMostCommonEmailAddress();
            for (int i = 0; i < 0; i++)
                result = DataBaseInterface.GetMostCommonEmailAddress();
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time it took databaserequest took : " + (elapsedMs));
            Outputbox.Text += "most common email domains: \n";
            for (int i = 0; i < Math.Min(_numberOfDomainsShown, result.Count); i++)
                Outputbox.Text += result[i].Key + " amount: " + result[i].Value + "\n";
        }

        private List<KeyValuePair<string, int>> CountDuplicates(List<string> toCount)
        {
            Dictionary<string, int> duplicateCounter = new Dictionary<string, int>();
            foreach (string element in toCount)
            {
                if (duplicateCounter.ContainsKey(element))
                    duplicateCounter[element]++;
                else
                    duplicateCounter.Add(element, 1);
            }
            List<KeyValuePair<string, int>> tosCounterResult = duplicateCounter.ToList();
            return tosCounterResult.AsParallel().OrderByDescending(x => x.Value).ToList();
        }

        private List<KeyValuePair<string,int>> GetCountysSize()
        {
            Dictionary<string, int> countysCounter = new Dictionary<string, int>();
            List<string> countys = DataBaseInterface.GetCountys();
            foreach (string county in countys)
            {
                if (countysCounter.ContainsKey(county))
                    countysCounter[county]++;
                else
                    countysCounter.Add(county, 1);
            }
            List<KeyValuePair<string, int>> countysCounterResult = countysCounter.ToList();


            return countysCounterResult.AsParallel().OrderByDescending(x => x.Value).ToList();
        }

        private List<KeyValuePair<KeyValuePair<string,string>, int>> GetLargestCountys()
        {

            Dictionary<KeyValuePair<string, string>, int> countysCounter = new Dictionary<KeyValuePair<string, string>, int>();
            List<KeyValuePair<string,string>> countys = DataBaseInterface.GetCountysWithPostcodes();
            foreach (KeyValuePair<string, string> county in countys)
            {
                if (countysCounter.ContainsKey(county))
                    countysCounter[county]++;
                else
                    countysCounter.Add(county, 1);
            }
            List<KeyValuePair<KeyValuePair<string, string>, int>> countysCounterResult = countysCounter.ToList();


            return  countysCounterResult.AsParallel().OrderByDescending(x => x.Value).ToList();
        }
        private (int count, (double longitude, double latitude)) GetClosestGroup(List<string> postcodes, double radius)
        {
            PostcodesIOClient pIOC = new MarkEmbling.PostcodesIO.PostcodesIOClient();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            List<Task<PostcodeResult>> PR = new List<Task<PostcodeResult>>();
            foreach (string postcode in postcodes)
                PR.Add(pIOC.LookupAsync(postcode));
            List<(double longitude, double latitude)> longLat = new List<(double longitude, double latitude)>();
            foreach (Task<PostcodeResult> result in PR)
            {
                while (!result.IsCompleted)
                    continue;
                if (result.Result == null)
                    continue;
                longLat.Add((result.Result.Longitude, result.Result.Latitude));;
            }
            int largestGroup = 0;
            (double longitude, double latitude) currentCenter = longLat[0];
            foreach ((double longitude, double latitude) ll in longLat)
            {
                int currentGroup = 0;
                foreach ((double longitude, double latitude) ll2 in longLat)
                {
                    if (getDistance(ll, ll2) < radius)
                        currentGroup++;
                }
                if (currentGroup > largestGroup)
                {
                    largestGroup = currentGroup;
                    currentCenter = ll;
                }
            }
            return (largestGroup, currentCenter);

        }
        private double getDistance((double longitude, double latitude) o1, (double longitude, double latitude) o2)
        {
            double R = 6371e3; // metres
            double φ1 = o1.latitude * (Math.PI / 180);
            double φ2 = o2.latitude * (Math.PI / 180);
            double Δφ = (o2.latitude - o1.latitude) * (Math.PI / 180);
            double Δλ = (o2.longitude - o1.longitude) * (Math.PI / 180);

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Gmap.Markers.Clear();
            (int count, (double longitude, double latitude)) result = GetClosestGroup(DataBaseInterface.GetAllPostCode(), int.Parse(Radius.Text));
            AmountText.Text = result.count + " people live within this circle";
            Gmap.MouseWheelZoomEnabled = true;
            Gmap.DragButton = System.Windows.Input.MouseButton.Left;
            Gmap.MapProvider = GMap.NET.MapProviders.GMapProviders.GoogleMap;
            Gmap.Position = new GMap.NET.PointLatLng(result.Item2.latitude, result.Item2.longitude);
            Gmap.Zoom = 5;
            Gmap.MaxZoom = 15;
            Gmap.MinZoom = 1;
            Gmap.ReloadMap();
            
            GMap.NET.WindowsPresentation.GMapMarker marker = new GMapMarker(new PointLatLng(result.Item2.latitude, result.Item2.longitude));
  
            marker.Shape = new Ellipse
            {
                Width = int.Parse(Radius.Text) /100,
                Height = int.Parse(Radius.Text) / 100,
                Stroke = Brushes.Green,
                StrokeThickness = 3
            };
            
            Gmap.Markers.Add(marker);
            UpdateLayout();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GetMostCommonDomains();
        }
    }


}

