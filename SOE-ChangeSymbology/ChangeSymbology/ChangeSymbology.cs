// Copyright 2018 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at <your Enterprise SDK install location>/userestrictions.txt.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.Specialized;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SOESupport;
using ESRI.ArcGIS.Display;

//TODO: sign the project (project properties > signing tab > sign the assembly)
//      this is strongly suggested if the dll will be registered using regasm.exe <your>.dll /codebase


namespace ChangeSymbology
{
    [ComVisible(true)]
    [Guid("b0081966-291d-4ed2-a046-9559632ea02f")]
    [ClassInterface(ClassInterfaceType.None)]
    [ServerObjectExtension("MapServer",//use "MapServer" if SOE extends a Map service and "ImageServer" if it extends an Image service.
        AllCapabilities = "Rerender",
        DefaultCapabilities = "",
        Description = "",
        DisplayName = "ChangeSymbology",
        Properties = "",
        SupportsREST = true,
        SupportsSOAP = false)]
    public class ChangeSymbology : IServerObjectExtension, IObjectConstruct, IRESTRequestHandler
    {
        private string soe_name;

        private IPropertySet configProps;
        private IServerObjectHelper serverObjectHelper;
        private ServerLogger logger;
        private IRESTRequestHandler reqHandler;

        private IFeatureLayer m_featureLayerToChange;
        private string m_mapLayerNameToChange;

        public ChangeSymbology()
        {
            soe_name = this.GetType().Name;
            logger = new ServerLogger();
            reqHandler = new SoeRestImpl(soe_name, CreateRestSchema()) as IRESTRequestHandler;
        }

        #region IServerObjectExtension Members

        public void Init(IServerObjectHelper pSOH)
        {
            serverObjectHelper = pSOH;
        }

        public void Shutdown()
        {
        }

        #endregion

        #region IObjectConstruct Members

        public void Construct(IPropertySet props)
        {
            configProps = props;
            //can put the logic here to access the layer since working with one layer only
            if (props.GetProperty("LayerName") != null)
            {
                m_mapLayerNameToChange = props.GetProperty("LayerName") as string;
            }
            else
            {
                throw new ArgumentNullException();
            }
            try
            {
                // Get the feature layer to be changed.
                // Since the layer is a property of the SOE, this only has to be done once.
                IMapServer3 mapServer = (IMapServer3)serverObjectHelper.ServerObject;
                string mapName = mapServer.DefaultMapName;
                IMapLayerInfo layerInfo;
                IMapLayerInfos layerInfos = mapServer.GetServerInfo(mapName).MapLayerInfos;
                // Find the index position of the map layer to query.
                int c = layerInfos.Count;
                int layerIndex = 0;
                for (int i = 0; i < c; i++)
                {
                    layerInfo = layerInfos.get_Element(i);
                    if (layerInfo.Name == m_mapLayerNameToChange)
                    {
                        layerIndex = i;
                        break;
                    }
                }
                // Use IMapServerDataAccess to get the data
                IMapServerDataAccess dataAccess = (IMapServerDataAccess)mapServer;
                // Get access to the source feature layer.
                m_featureLayerToChange = (dataAccess.GetDataSource(mapName, layerIndex)) as IFeatureLayer;
            }
            catch
            {
                logger.LogMessage(ServerLogger.msgType.error, "Construct", 8000,
                    "SOE custom error: Could not get the feature layer.");
            }
        }

        #endregion

        #region IRESTRequestHandler Members

        public string GetSchema()
        {
            return reqHandler.GetSchema();
        }

        public byte[] HandleRESTRequest(string Capabilities, string resourceName, string operationName, string operationInput, string outputFormat, string requestProperties, out string responseProperties)
        {
            return reqHandler.HandleRESTRequest(Capabilities, resourceName, operationName, operationInput, outputFormat, requestProperties, out responseProperties);
        }

        #endregion

        private RestResource CreateRestSchema()
        {
            RestResource rootRes = new RestResource(soe_name, false, RootResHandler);

            RestOperation sampleOper = new RestOperation("Rerender",
                                                      new string[] { "Color" },
                                                      new string[] { "json" },
                                                      RerenderOperHandler);

            rootRes.operations.Add(sampleOper);
            
            return rootRes;
        }

        private byte[] RootResHandler(NameValueCollection boundVariables, string outputFormat, string requestProperties, out string responseProperties)
        {
            responseProperties = null;

            JsonObject result = new JsonObject();
            result.AddString("Change that Color to", "red, green or blue");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] SampleOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            string parm1Value;
            bool found = operationInput.TryGetString("parm1", out parm1Value);
            if (!found || string.IsNullOrEmpty(parm1Value))
                throw new ArgumentNullException("parm1");

            //string parm2Value;
            //found = operationInput.TryGetString("parm2", out parm2Value);
            //if (!found || string.IsNullOrEmpty(parm2Value))
            //    throw new ArgumentNullException("parm2");

            JsonObject result = new JsonObject();
            result.AddString("parm1", parm1Value);
            //result.AddString("parm2", parm2Value);

            return Encoding.UTF8.GetBytes(result.ToJson());
        }

        private byte[] RerenderOperHandler(NameValueCollection boundVariables,
                                                  JsonObject operationInput,
                                                      string outputFormat,
                                                      string requestProperties,
                                                  out string responseProperties)
        {
            responseProperties = null;

            changeRenderer(m_featureLayerToChange, "Red");

            JsonObject result = new JsonObject();

            result.AddString("Method Executed!", "yayyyyy!");

            return Encoding.UTF8.GetBytes(result.ToJson());
        }


        private void changeRenderer(IFeatureLayer fl, string color)
        {

            IGeoFeatureLayer geoFl = (IGeoFeatureLayer)fl;
            SimpleRenderer featureRenderer = (SimpleRenderer)geoFl.Renderer;

            ISimpleMarkerSymbol simpleMarkerSymbol = new SimpleMarkerSymbolClass();

            int red = 0;
            int green = 0;
            int blue = 0;

            switch (color)
            {
                case "Red":
                    red = 255;
                    green = 0;
                    blue = 0;
                    break;

                case "Green":
                    red = 0;
                    green = 255;
                    blue = 0;
                    break;

                case "Blue":
                    red = 0;
                    green = 0;
                    blue = 255;
                    break;

            }

            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = red;
            rgbColor.Green = green;
            rgbColor.Blue = blue;

            simpleMarkerSymbol.Color = rgbColor;
            simpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSCircle;

            featureRenderer.Symbol = (ISymbol)simpleMarkerSymbol;

        }
    }
}
