using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Fabric;

namespace AkkaClusterTest
{

    public class AkkaProperties
    {
        private string _akkaClusterPropertyName = "AkkaCluster";
        private const string _seedPropertyName = "SeedNodes";
        private string _applicationName = "";
        private FabricClient.PropertyManagementClient _propertyManager = null;

        public Uri GetSeedNodesPropertyUri()
        {
            var akkaClusterPropertyPath = _akkaClusterPropertyName + "/" + _seedPropertyName;
            return (_applicationName.Length == 0 ? new Uri("Fabric:/" + akkaClusterPropertyPath) : new Uri("Fabric:/" + _applicationName + "/" + akkaClusterPropertyPath));
        }

        public Uri GetAkkaClusterPropertyUri()
        {
            return (_applicationName.Length == 0 ? new Uri("Fabric:/" + _akkaClusterPropertyName) : new Uri("Fabric:/" + _applicationName + "/" + _akkaClusterPropertyName));
        }


        #region public methods
        public string AkkaClusterPropertyName
        {
            get { return _akkaClusterPropertyName; }
            set { _akkaClusterPropertyName = value; }
        }

        public AkkaProperties()
        {
            _propertyManager = new FabricClient().PropertyManager;
        }
        public AkkaProperties(string appName)
        {
            _propertyManager = new FabricClient().PropertyManager;
            _applicationName = appName;

        }

        public void Initialize()
        {
            try
            {
                InitializePropertyName(GetAkkaClusterPropertyUri());
                InitializePropertyName(GetSeedNodesPropertyUri());
            }
            catch (Exception e)
            {
                // something went wrong
                throw;
            }
        }


        public async Task<string> SetSeedIP(string seedIP)
        {
            var seedName = "Seed_" + Guid.NewGuid().ToString();
            // using batch property so more operations can be easily added.
            var propertyOperations = new List<PropertyBatchOperation>
            {
                new PutPropertyOperation(seedName, seedIP),
            };

            try
            {
                var result = await _propertyManager.SubmitPropertyBatchAsync(GetSeedNodesPropertyUri(), propertyOperations);

                // Note; these indexes are tied to the batch operation array above.  If you adjust
                // the set of operations, need to update these offsets accordingly
                if (result.FailedOperationIndex == -1)
                {
                    //update successed
                }
                else
                {
                    var message = String.Format("SetSeedIP {0}:{1} failed", seedName, seedIP);
                    throw new Exception(message);
                }
                return seedName;
            }
            catch (Exception ex0)
            {
                throw;
            }
        }

        public void Remove(string seedName)
        {
            try
            {
                var parentName = GetSeedNodesPropertyUri();
                _propertyManager.DeletePropertyAsync(parentName, seedName);
            }
            catch
            {
                throw;
            }
        }

        public void CleanUp()
        {
            var properties = _propertyManager.EnumeratePropertiesAsync(GetSeedNodesPropertyUri(), false, null).Result;
            foreach (var prop in properties)
                _propertyManager.DeletePropertyAsync(GetSeedNodesPropertyUri(), prop.Metadata.PropertyName).Wait();


            properties = _propertyManager.EnumeratePropertiesAsync(GetAkkaClusterPropertyUri(), false, null).Result;
            foreach (var prop in properties)
                _propertyManager.DeletePropertyAsync(GetAkkaClusterPropertyUri(), prop.Metadata.PropertyName).Wait();


        }
        #endregion

        #region Private methods

        private void InitializePropertyName(Uri uri)
        {
            if (_propertyManager.NameExistsAsync(uri).Result == false)
            {
                _propertyManager.CreateNameAsync(uri).Wait();
            }
        }

        public String[] getSeedIPs()
        {


            var result = _propertyManager.EnumeratePropertiesAsync(GetSeedNodesPropertyUri(), true, null).Result;


            return result.Select(e => e.GetValue<String>()).ToArray();
        }

        #endregion
    }
}
