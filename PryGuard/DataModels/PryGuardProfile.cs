
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using PryGuard.Resources.Helpers;
using PryGuard.Core.Browser.Model.Configs;


namespace PryGuard.DataModels
{
    public class PryGuardProfile : INotifyPropertyChanged
    {
        private ProxySettings _proxy;
        public ProxySettings Proxy
        {
            get => _proxy;
            set
            {
                if (_proxy == value)
                    return;
                _proxy = value;
                OnPropertyChanged(nameof(Proxy));
            }
        }
        public bool IsSaved { get; set; } = false;

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                // Rename folder at once
                if (_name != null && Directory.Exists(ClientConfig.ChromeDataPath + "\\" + _name))
                {
                    Directory.Move(ClientConfig.ChromeDataPath + "\\" + _name, ClientConfig.ChromeDataPath + "\\" + value);
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _cookies;
        public string Cookies
        {
            get => _cookies;
            set
            {
                if (_cookies == value)
                    return;
                _cookies = value;
                OnPropertyChanged(nameof(Cookies));
            }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                if (_id == value)
                    return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status == value)
                    return;
                _status = value;
                _status = _status.Replace("System.Windows.Controls.ComboBoxItem: ", "");
                OnPropertyChanged(nameof(Status));
            }
        }

        private string _tags;
        public string Tags
        {
            get => _tags;
            set
            {
                if (_tags == value)
                    return;
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private FakeProfile _fakeProfile;
        public FakeProfile FakeProfile
        {
            get => _fakeProfile;
            set
            {
                if (_fakeProfile == value)
                    return;
                _fakeProfile = value;
                OnPropertyChanged(nameof(FakeProfile));
            }
        }

        private bool _isLoadImage;
        public bool IsLoadImage
        {
            get => _isLoadImage;
            set
            {
                if (_isLoadImage == value)
                    return;
                _isLoadImage = value;
                OnPropertyChanged(nameof(IsLoadImage));
            }
        }

        private bool _isLoadCacheInMemory;
        public bool IsLoadCacheInMemory
        {
            get => _isLoadCacheInMemory;
            set
            {
                if (_isLoadCacheInMemory == value)
                    return;
                _isLoadCacheInMemory = value;
                OnPropertyChanged(nameof(IsLoadCacheInMemory));
            }
        }

        private bool _isAdBlock;
        public bool IsAdBlock
        {
            get => _isAdBlock;
            set
            {
                if (_isAdBlock == value)
                    return;
                _isAdBlock = value;
                OnPropertyChanged(nameof(IsAdBlock));
            }
        }

        private string _cachePath;
        public string CachePath
        {
            get => _cachePath;
            set
            {
                if (_cachePath == value)
                    return;
                _cachePath = value;
                OnPropertyChanged(nameof(CachePath));
            }
        }

        private static int _nextId;

        static PryGuardProfile()
        {
            _nextId = LoadLastUsedId();
        }

        private static int LoadLastUsedId()
        {
            int lastId = 0;
            string path = Path.Combine(ClientConfig.ChromeDataPath, "LastUsedId.txt");

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                if (int.TryParse(content, out lastId))
                {
                    return lastId;
                }
            }
            return lastId;
        }

        private static void SaveLastUsedId(int lastId)
        {
            string path = Path.Combine(ClientConfig.ChromeDataPath, "LastUsedId.txt");

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, lastId.ToString());
        }

        private static int GenerateUniqueId()
        {
            int newId = Interlocked.Increment(ref _nextId);
            SaveLastUsedId(newId);
            return newId;
        }


        public static PryGuardProfile GenerateNewProfile(string name)
        {
            int uniqueId = GenerateUniqueId(); // Generate the unique ID once
            return new PryGuardProfile()
            {
                Name = name,
                Id = uniqueId, // Use the same unique ID here
                Status = "NEW",
                FakeProfile = FakeProfileFactory.Generate(),
                IsEnabled = false,
                IsAdBlock = true,
                IsLoadImage = true,
                IsLoadCacheInMemory = true,
                CachePath = Path.Combine(ClientConfig.ChromeDataPath, $"{name}_Cache_{uniqueId}"), // And use it here
                Proxy = new ProxySettings()
            };
        }

        public static PryGuardProfile ImportFromProfile(PryGuardProfile existingProfile)
        {
            int uniqueId = GenerateUniqueId();
            return new PryGuardProfile()
            {
                Name = existingProfile.Name + " (Copy)",
                Id = uniqueId,
                Status = "NEW",
                FakeProfile = existingProfile.FakeProfile, // Assuming Clone() is implemented
                IsEnabled = existingProfile.IsEnabled,
                IsAdBlock = existingProfile.IsAdBlock,
                IsLoadImage = existingProfile.IsLoadImage,
                IsLoadCacheInMemory = existingProfile.IsLoadCacheInMemory,
                CachePath = Path.Combine(ClientConfig.ChromeDataPath, $"{existingProfile.Name}_Cache_{uniqueId}"),
                Proxy = existingProfile.Proxy // Assuming Clone() is implemented
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}