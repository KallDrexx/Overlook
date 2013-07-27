using System;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Overlook.Common.Data;
using Version = Lucene.Net.Util.Version;

namespace Overlook.Server.Storage
{
    public class LuceneStorageEngine : IStorageEngine
    {
        private readonly IndexWriter _indexWriter;
        private readonly Directory _directory;

        public LuceneStorageEngine(Directory directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");

            _directory = directory;
            _indexWriter = new IndexWriter(_directory, new StandardAnalyzer(Version.LUCENE_30), false, IndexWriter.MaxFieldLength.LIMITED);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void StoreSnapshot(Snapshot snapshot1)
        {
            throw new NotImplementedException();
        }

        public Snapshot[] GetSnapshots(DateTime startSearchDate, DateTime endSearchDate)
        {
            throw new NotImplementedException();
        }
    }
}
