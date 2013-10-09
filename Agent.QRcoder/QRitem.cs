using System.Collections;

namespace Agent.QRcoder
{
    public class QRitems
    {
        private ArrayList AllItems = new ArrayList();
        private int CurrentIndex = 0;

        public int Count { get { return AllItems.Count; } }
        
        public void AddQRcode(string name, string content)
        {
            AllItems.Add(new QRitem(name, content));
        }

        public QRitem GetCurrent()
        {
            return (QRitem)AllItems[CurrentIndex];
        }

        public void GetNext()
        {
            if (CurrentIndex == AllItems.Count - 1)
                CurrentIndex = 0;
            else
                CurrentIndex++;
        }

        public void GetPrevious()
        {
            if (CurrentIndex == 0)
                CurrentIndex = AllItems.Count - 1;
            else
                CurrentIndex--;
        }
    }

    public class QRitem
    {
        public string Name;
        public string Content;

        public QRitem(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }



}
