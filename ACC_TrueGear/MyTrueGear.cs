using TrueGearSDK;


namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        public TrueGearMod() 
        {
            _player = new TrueGearPlayer("805550","Assetto Corsa Competizione");
            //_player = new TrueGearPlayer();
            //RegisterFilesFromDisk();
            _player.Start();
        }

        //private void RegisterFilesFromDisk()
        //{
        //    FileInfo[] files = new DirectoryInfo(".//TrueGear")   //  (".//BepInEx//plugins//TrueGear")
        //            .GetFiles("*.asset_json", SearchOption.AllDirectories);

        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        string name = files[i].Name;
        //        string fullName = files[i].FullName;
        //        if (name == "." || name == "..")
        //        {
        //            continue;
        //        }
        //        string jsonStr = File.ReadAllText(fullName);
        //        JSONNode jSONNode = JSON.Parse(jsonStr);
        //        EffectObject _curAssetObj = EffectObject.ToObject(jSONNode.AsObject);
        //        string uuidName = Path.GetFileNameWithoutExtension(fullName);
        //        _curAssetObj.uuid = uuidName;
        //        _curAssetObj.name = uuidName;
        //        _player.SetupRegister(uuidName, jsonStr);
        //    }
        //}

        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }

    }
}
