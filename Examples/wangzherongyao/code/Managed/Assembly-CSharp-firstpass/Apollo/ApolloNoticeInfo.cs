namespace Apollo
{
    using System;

    public class ApolloNoticeInfo : ApolloStruct<ApolloNoticeInfo>
    {
        private ListView<ApolloNoticeData> dataList;

        public void Dump()
        {
            Console.WriteLine("*******************notice info begin*****************");
            Console.WriteLine("size of info list:{0}", this.DataList.Count);
            Console.WriteLine("*******************notice info end*******************");
        }

        public override ApolloNoticeInfo FromString(string src)
        {
            Console.WriteLine("src={0}", src);
            char[] separator = new char[] { ',' };
            string[] strArray = src.Split(separator);
            this.DataList.Clear();
            foreach (string str in strArray)
            {
                ApolloNoticeData item = new ApolloNoticeData();
                item.FromString(str);
                this.DataList.Add(item);
            }
            this.Dump();
            return this;
        }

        public void Reset()
        {
            this.dataList.Clear();
        }

        public ListView<ApolloNoticeData> DataList
        {
            get
            {
                if (this.dataList == null)
                {
                    this.dataList = new ListView<ApolloNoticeData>();
                }
                return this.dataList;
            }
        }
    }
}

