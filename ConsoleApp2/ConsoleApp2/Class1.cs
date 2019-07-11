using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    [Serializable]
    class Student
    {
        public string name, phone;
    }

    //파일 정보를 저장하는 클래스 정의
    [Serializable]
    class File_info
    {
        public string filename;
        public long filesize;  //C#에서는 파일 크기를 long으로 받음
    }
}
