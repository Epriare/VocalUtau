﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using VocalUtau.DirectUI.Utils.SingerUtils;
using VocalUtau.Formats.Model.Database;
using VocalUtau.Formats.Model.Utils;
using VocalUtau.Formats.Model.VocalObject;

namespace VocalUtau.ActionWorker
{
    public class SystemSingerWorker
    {
        SingerIndexer _Indexer = new SingerIndexer();

        public SingerIndexer Indexer
        {
            get { return _Indexer; }
            set { _Indexer = value; }
        }
        public SystemSingerWorker()
        {
        }

        public void FillSingerIn(List<SingerObject> ListBeFilled)
        {
            foreach (SingerObject so in Program.GlobalPackage.Configures.GlobalSingerList)
            {
                if (ListBeFilled.IndexOf(so) == -1)
                {
                    SingerObject ss = (SingerObject)so.Clone();
                    ss.IsSystemSinger = true;
                    ListBeFilled.Add(ss);
                }
                else
                {
                    string OldGUID = ListBeFilled[ListBeFilled.IndexOf(so)].getGuid();
                    ListBeFilled[ListBeFilled.IndexOf(so)] = (SingerObject)so.Clone();
                    ListBeFilled[ListBeFilled.IndexOf(so)].GUID = OldGUID;
                    ListBeFilled[ListBeFilled.IndexOf(so)].IsSystemSinger = true;
                }
            }
        }

        private void InitNewSinger(string objPath)
        {
           
        }
        public void UpdateGlobalSingerList()
        {
            if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\voicedb"))
            {
                System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\voicedb");
            }
            string voicedir = AppDomain.CurrentDomain.BaseDirectory + "\\voicedb";
            List<FileInfo> flist = PathUtils.GetFirstFiles(new System.IO.DirectoryInfo(voicedir), new string[] { "voicedbi.dat", "character.txt" }, Program.GlobalPackage.Configures.VocalDbSearchDeep);
            
            Object locker=new Object();
            Parallel.For(0, flist.Count, (i) => {
                bool Haved = false;
                foreach(SingerObject s in Program.GlobalPackage.Configures.GlobalSingerList)
                {
                    if (s.getRealSingerFolder() == flist[i].Directory.FullName)
                    {
                        Haved = true;
                        break;
                    }
                }
                if (!Haved)
                {
                    VocalIndexObject vio = VocalIndexObject.Deseralize(flist[i].Directory.FullName);
                    SingerObject nso = new SingerObject();
                    nso.VocalName = vio.CharacertData.Dbname;
                    nso.SingerFolder = PathUtils.RelativePath(AppDomain.CurrentDomain.BaseDirectory, PathUtils.AbsolutePath(voicedir, flist[i].Directory.FullName)); 
                    nso.Avatar = vio.CharacertData.Avatar;
                    nso.PartResampler = "resampler.exe";
                    lock (locker)
                    {
                        _Indexer.AddIndexer(flist[i].Directory.FullName, vio);
                        if (vio.GUID != "") nso.GUID = vio.GUID;
                        if (Program.GlobalPackage.Configures.GlobalSingerList.IndexOf(nso) == -1)
                        {
                            Program.GlobalPackage.Configures.GlobalSingerList.Add(nso);
                        }
                        else
                        {
                            Program.GlobalPackage.Configures.GlobalSingerList[Program.GlobalPackage.Configures.GlobalSingerList.IndexOf(nso)] = nso;
                        }
                    }
                }
            });
        }
        public void RemoveUnuseSinger(List<SingerObject> SingerList = null)
        {
            if (SingerList == null) SingerList = Program.GlobalPackage.Configures.GlobalSingerList;
            //Check Unusful
        }
        public void InitSingers(List<SingerObject> SingerList = null)
        {
            if (SingerList == null) SingerList = Program.GlobalPackage.Configures.GlobalSingerList;
            foreach (SingerObject so in SingerList)
            {
                if (so == null) continue;
                string RFolder=so.getRealSingerFolder();
                _Indexer.LoadSinger(RFolder);
                VocalIndexObject vio=_Indexer.getIndex(RFolder);
                if (vio != null)
                {
                    if (vio.GUID != "") so.GUID = vio.GUID;
                }
            }
        }
    }
}
