using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

/*
 * TCP 서버 - 심플 클라우드 서버
 * 접속한 클라이언트에게 파일목록 제공
 * 클라이언트가 요청한 파일을 다운로드
 * 클라이언트가 업로드하는 파일을 하드디스크에 저장
 */

namespace server
{
    sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            //파일목록 로드
            List<File_info> file_fist = new List<File_info>();
            DirectoryInfo info = new DirectoryInfo("./files");
            //DirectoryInfo : 특정 폴더안에 있는 파일을 접근/ 폴더 생성, 이동 할때 사용하는 클래스
            //GetFiles : 접근한 폴더안에 있는 파일정보들을 추출하는 메소드

            //var : 프로그램이 시작하기전 해당 변수에 대입된 자료형/클래스를 보고 자동으로 자료형/클래스 맞게 변수를 선언해주는 문법
            //foreach (FileInfo fileInfo in info.GetFiles())
            foreach (var fileInfo in info.GetFiles())
            {

            }

            //서버 생성 - 10000번 포트 사용
            TcpListener listener = new TcpListener(11000);

            //클라이언트 연결 허용
            listener.Start();
            //무한반복 - 여러 클라이언트 접속 처리용도
            //클라이언트 연결
            //반복 - 클라이언트가 연결을 끊을때까지
                //
            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();

            //파일데이터를 저장할 파일스트림 객체 생성
            FileStream file_Stream = File.Create("./download.txt");
            //수신받은 데이터를 파일스트림에 저장
            byte[] recv_data = new byte[1024];
            int recv_size = 0;
            while ((recv_size = stream.Read(recv_data, 0, recv_data.Length)) > 0)
            {
                file_Stream.Write(recv_data, 0, recv_size);

            }
            //파일 닫기
            file_Stream.Close();
            //클라이언트 연결 종료
            stream.Close();
            client.Close();

            //서버종료
            listener.Stop();

        }


    }

}