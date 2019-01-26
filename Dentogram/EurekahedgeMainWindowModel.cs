using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dentogram.Clustering;
using Dentogram.EurekahedgeParcing;

namespace Dentogram
{
    public class EurekahedgeMainWindowModel : ViewModelBase
    {
        private const string EurekahedgeSource = @"D:\GitHub2\Eurekahedge\Dentogram\eurekahedge.txt";
        private const string EurekahedgeResult = @"D:\GitHub2\Eurekahedge\Dentogram\eurekahedge result.txt";
        private const string EurekahedgeResultOld = @"D:\GitHub2\Eurekahedge\Dentogram\eurekahedge result old.txt";
        private const string EurekahedgeResultAntlr = @"D:\GitHub2\Eurekahedge\Dentogram\eurekahedge result antlr.txt";

        private List<string> files;
        private List<string> textes;
        private List<string> parsedRegions;
        private List<string> dataSets;

        private List<int> shindels;
        public List<int> Shindels
        {
            get { return shindels; }
            set
            {
                shindels = value;
                OnPropertyChanged(nameof(Shindels));
            }
        }

        private int activeShindel;
        public int ActiveShindel
        {
            get { return activeShindel; }
            set
            {
                activeShindel = value;
                OnPropertyChanged(nameof(ActiveShindel));

                ClusterDistance.Shindel = value;
            }
        }

        private List<ClusterDistance.Strategy> strateges;
        public List<ClusterDistance.Strategy> Strateges
        {
            get { return strateges; }
            set
            {
                strateges = value;
                OnPropertyChanged(nameof(Strateges));
            }
        }

        private ClusterDistance.Strategy activeStratege;
        public ClusterDistance.Strategy ActiveStratege
        {
            get { return activeStratege; }
            set
            {
                activeStratege = value;
                OnPropertyChanged(nameof(ActiveStratege));
            }
        }

        private List<string> modes;
        public List<string> Modes
        {
            get { return modes; }
            set
            {
                modes = value;
                OnPropertyChanged(nameof(Modes));
            }
        }

        private string activeMode;
        public string ActiveMode
        {
            get { return activeMode; }
            set
            {
                activeMode = value;
                OnPropertyChanged(nameof(ActiveMode));

                ClusterDistance.Mode = value;
            }
        }

        private Node selectedNode;
        public Node SelectedNode
        {
            get { return selectedNode; }
            set
            {
                if (selectedNode == value)
                {
                    return;
                }
                selectedNode = value;

                if (IsCheckedText1)
                {
                    Header1 = SelectedNode.Name;
                    ClusterText1 = SelectedNode.Text;
                    //Text1 = LoadFile(SelectedNode.Name);
                }
                else if (IsCheckedText2)
                {
                    Header2 = SelectedNode.Name;
                    ClusterText2 = SelectedNode.Text;
                    //Text2 = LoadFile(SelectedNode.Name);
                }
            }
        }

        private bool isCheckedText1 = true;
        public bool IsCheckedText1
        {
            get { return isCheckedText1; }
            set
            {
                if (value == isCheckedText1)
                {
                    return;
                }
                isCheckedText1 = value;
                OnPropertyChanged(nameof(IsCheckedText1));

                IsCheckedText2 = !value;
            }
        }

        private bool isCheckedText2;
        public bool IsCheckedText2
        {
            get { return isCheckedText2; }
            set
            {
                if (value == isCheckedText2)
                {
                    return;
                }
                isCheckedText2 = value;
                OnPropertyChanged(nameof(IsCheckedText2));

                IsCheckedText1 = !value;
            }
        }

        private List<Node> items;
        public List<Node> Items
        {
            get { return items; }
            set
            {
                items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private string header1;
        public string Header1
        {
            get { return header1; }
            set
            {
                header1 = value;
                OnPropertyChanged(nameof(Header1));
            }
        }

        private string header2;
        public string Header2
        {
            get { return header2; }
            set
            {
                header2 = value;
                OnPropertyChanged(nameof(Header2));
            }
        }

        private string clusterText1;
        public string ClusterText1
        {
            get { return clusterText1; }
            set
            {
                clusterText1 = value;
                OnPropertyChanged(nameof(ClusterText1));
            }
        }

        private string clusterText2;
        public string ClusterText2
        {
            get { return clusterText2; }
            set
            {
                clusterText2 = value;
                OnPropertyChanged(nameof(ClusterText2));
            }
        }

        private string text1;
        public string Text1
        {
            get { return text1; }
            set
            {
                text1 = value;
                OnPropertyChanged(nameof(Text1));
            }
        }
        
        private string text2;
        public string Text2
        {
            get { return text2; }
            set
            {
                text2 = value;
                OnPropertyChanged(nameof(Text2));
            }
        }

        private string filesDescription;
        public string FilesDescription
        {
            get { return filesDescription; }
            set
            {
                filesDescription = value;
                OnPropertyChanged(nameof(FilesDescription));
            }
        }

        public EurekahedgeMainWindowModel()
        {
            Modes = new List<string>(ClusterDistance.AllModes);
            ActiveMode = "JaccardDistance";

            Strateges = new List<ClusterDistance.Strategy>()
            {
                ClusterDistance.Strategy.MinLinkage,
                ClusterDistance.Strategy.MaxLinkage,
                ClusterDistance.Strategy.AverageLinkage,
                ClusterDistance.Strategy.AverageLinkageWeighted,
            };
            ActiveStratege = ClusterDistance.Strategy.AverageLinkage;

            var t = new List<int>();
            for (int i = 1; i < 40; i++)
            {
                t.Add(i);
            }

            Shindels = t;
            ActiveShindel = 5;

            FilesDescription = "Wait. Loading files...";

            Task.Run(() =>
            {
                //LoadFilesOld();
                LoadFiles();
                LoadFilesAntlr();
                //Start();
            });
        }


        private void LoadFilesOld()
        {
            try
            {
                var inputTextes = GetTextes().Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                
                var sw = Stopwatch.StartNew();
                
                EurekahedgeLockupParcerOld parcer = new EurekahedgeLockupParcerOld();
                var fileInfosAll = inputTextes
                    .Select(x => new
                    {
                        lockUp = parcer.Parce(x), 
                        parceResult = parcer.Replace(x), 
                        text = x,
                    })
                    //.Where(x => x.lockUp.Warning)
                    .GroupBy(x => x.parceResult)
                    .Select(x => new
                    {
                        text = x.First().parceResult,
                        //text = x.text,
                        sourceText = x.First().text,
                        firstSourceText = x.First().text,
                        firstLockUp = x.First().lockUp,
                        count = x.Count(),
                    })
                    .ToList();

                var fileInfos = fileInfosAll
                    .Take(100)
                    .ToList();
            
                sw.Stop();
                TimeSpan timeParsing = sw.Elapsed;
                
                textes = fileInfos.Select(x => x.text).ToList();
                files = fileInfos.Select(x => $"{x.text}[{x.count}]").ToList();
                parsedRegions = fileInfos.Select(x => x.sourceText).ToList();
                dataSets = fileInfos.Select(x => x.text).ToList();
                
                File.WriteAllLines(EurekahedgeResultOld, fileInfosAll.Select(x => $"{x.text}\t{x.firstSourceText}\t{x.firstLockUp.ToTestString()}\t{x.count}"));
                
                FilesDescription = $"All lines: {inputTextes.Count}; All parced lines: {fileInfosAll.Count}; Time: {timeParsing:mm\\:ss}";

            }
            catch (Exception e)
            {
                Debug.Fail("LoadFiles");
                Console.WriteLine(e);
                throw;
            }

        }

        private void LoadFilesAntlr()
        {
            try
            {
                var inputTextes = GetTextes().Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                
                //inputTextes = Enumerable.Repeat(inputTextes, 1000).SelectMany(x => x).ToList();

                var sw = Stopwatch.StartNew();
                
                EurekahedgeLockupParcerAntlr parcer = new EurekahedgeLockupParcerAntlr();
                var fileInfosAll = inputTextes
                    .Select(x => new
                    {
                        lockUp = parcer.Parce(x), 
                        //parceResult = parcer.Replace(x), 
                        text = x,
                    })
                    //.Where(x => x.lockUp.Warning)
                    .Select(x => new
                    {
                        text = x.text,
                        //text = x.text,
                        sourceText = x.text,
                        firstSourceText = x.text,
                        firstLockUp = x.lockUp,
                        count = 1,
                    })
                    .ToList();

                var fileInfos = fileInfosAll
                    .Take(100)
                    .ToList();
            
                sw.Stop();
                TimeSpan timeParsing = sw.Elapsed;
                
                textes = fileInfos.Select(x => x.text).ToList();
                files = fileInfos.Select(x => $"{x.text}[{x.count}]").ToList();
                parsedRegions = fileInfos.Select(x => x.sourceText).ToList();
                dataSets = fileInfos.Select(x => x.text).ToList();
                
                File.WriteAllLines(EurekahedgeResultAntlr, fileInfosAll.Select(x => $"{x.text}\t{x.firstSourceText}\t{x.firstLockUp.ToTestString()}\t{x.count}"));
                
                FilesDescription = $"All lines: {inputTextes.Count}; All parced lines: {fileInfosAll.Count}; Time: {timeParsing:mm\\:ss}";

            }
            catch (Exception e)
            {
                Debug.Fail("LoadFiles");
                Console.WriteLine(e);
                throw;
            }

        }

        private void LoadFiles()
        {
            try
            {
                //var sw = Stopwatch.StartNew();
                

                //var inputTextes = Enumerable.Repeat(GetTextes().Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList(), 1000)
                var inputTextes = GetTextes().Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                //inputTextes = Enumerable.Repeat(inputTextes, 200).SelectMany(x => x).ToList();

                /*
                var fileInfosAll = inputTextes.
                    Select(x => new
                        {
                            lockUp = parcer.Parce(x), 
                            parceResult = parcer.Replace(x), 
                            text = x,
                        }).
                    //GroupBy(x => !string.IsNullOrEmpty(x.parceResult) ? x.parceResult : x.text).
                    GroupBy(x => !string.IsNullOrEmpty(x.parceResult) ? x.parceResult : x.text).
                    Select(x => new
                        {
                            text = x.Key,
                            sourceText = string.Join(Environment.NewLine, x.Select(y => y.text).Distinct()),
                            firstSourceText = x.First().text,
                            firstLockUp = x.First().lockUp,
                            count = x.Count(),
                        }).
                    ToList();
                    */
                    
                var sw = Stopwatch.StartNew();
                
                EurekahedgeLockupParcer parcer = new EurekahedgeLockupParcer();
                var fileInfosAll = inputTextes
                    .Select(x => new
                    {
                        lockUp = parcer.Parce(x), 
                        parceResult = parcer.Replace(x), 
                        text = x,
                    })
                    //.Where(x => x.lockUp.Warning)
                    /*.GroupBy(x => x.parceResult)
                    .Select(x => new
                    {
                        text = x.First().parceResult,
                        //text = x.text,
                        sourceText = x.First().text,
                        firstSourceText = x.First().text,
                        firstLockUp = x.First().lockUp,
                        count = x.Count(),
                    })
                    */
                    .Select(x => new
                    {
                        text = x.parceResult,
                        //text = x.text,
                        sourceText = x.text,
                        firstSourceText = x.text,
                        firstLockUp = x.lockUp,
                        count = 1,
                    })
                    .ToList();

                var fileInfos = fileInfosAll
                    .Take(100)
                    .ToList();
            
                sw.Stop();
                TimeSpan timeParsing = sw.Elapsed;
                
                textes = fileInfos.Select(x => x.text).ToList();
                files = fileInfos.Select(x => $"{x.text}[{x.count}]").ToList();
                parsedRegions = fileInfos.Select(x => x.sourceText).ToList();
                dataSets = fileInfos.Select(x => x.text).ToList();
                
                File.WriteAllLines(EurekahedgeResult, fileInfosAll.Select(x => $"{x.text}\t{x.firstSourceText}\t{x.firstLockUp.ToTestString()}\t{x.count}"));
                
                FilesDescription = $"All lines: {inputTextes.Count}; All parced lines: {fileInfosAll.Count}; Time: {timeParsing:mm\\:ss}";

            }
            catch (Exception e)
            {
                Debug.Fail("LoadFiles");
                Console.WriteLine(e);
                throw;
            }
        }

        private IEnumerable<string> GetTextes()
        {
            int fileIndex = 1;
            using (var fileStream = new FileStream(EurekahedgeSource, FileMode.Open))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string line = reader.ReadLine();
                    
                    while (line != null)
                    {
                        fileIndex++;
                        yield return line;

                        if (fileIndex % 100 == 0)
                        {
                            FilesDescription = $"Loading files[{fileIndex}]...";
                        }
                        line = reader.ReadLine();
                    }
                }
            }
        }

        public void Start()
        {

            if (textes.Count < 3)
            {
                return;
            }

            ClusteringTreeModel clusteringModel = new ClusteringTreeModel(dataSets, parsedRegions, textes, files);
            ClusterNodeCollection clusters = clusteringModel.ExecuteClustering(ActiveStratege, 1);
            
            Items = new List<Node> { BuildRootNode(clusters.FirstOrDefault()) };
        }
        
        private Node BuildRootNode(ClusterNode cluster)
        {
            Node child0 = null;
            Node child1 = null;
            
            if (cluster.NodesCount == 0)
            {
                if (cluster.LeafsCount == 1)
                {
                    return GetNodeFromCluster(cluster.Leafs[0]);
                }
                if (cluster.LeafsCount == 2)
                {
                    child0 = GetNodeFromCluster(cluster.Leafs[0]);
                    child1 = GetNodeFromCluster(cluster.Leafs[1]);
                }
            }
            else if (cluster.NodesCount == 1)
            {
                child0 = GetNodeFromCluster(cluster.Leafs[0]);
                child1 = BuildRootNode(cluster.Nodes[0]);
            }
            else
            {
                child0 = BuildRootNode(cluster.Nodes[0]);
                child1 = BuildRootNode(cluster.Nodes[1]);
            }

            return Create(child0, child1, cluster.Distance.ToString("F"));
        }

        private Node GetNodeFromCluster(ClusterLeaf pattern)
        {
            return new Node(pattern.FileName) { Text = pattern.Region };
        }

        private Node Create(Node child0, Node child1, string name)
        {
            return new Node(child0, child1) { Name = name };
        }
    }
}