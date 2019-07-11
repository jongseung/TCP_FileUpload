using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using data_struct;

/*
 * TCP 클라이언트 - 심플 클라우드 클라이언트
 * 기능
 * 서버 연결 후 다운 받을 수 있는 파일목록 출력
 * 서버에 파일 다운로드, 업로드 요청 가능
 */
namespace client
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
            //서버연결
            TcpClient client = new TcpClient("127.0.0.1", 11000);

            //반복문에서 사용할 파일 스트림, 바이트배열등 선언
            byte[] datas = new byte[1024];
            int data_size, i;
            NetworkStream net_stream = client.GetStream();
            FileStream file_stream;
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
            //반복 - 연결을 종료하는 입력이 들어올때까지
            for(; ; )
            {
                //파일목록을 수신
                File_info[] file_list = (File_info[])formatter.Deserialize(net_stream);
                //반복 - 파일목록을 콘솔창에 출력
                Console.WriteLine("번호\t\t파일명\t\t파일크기");
                Console.WriteLine("-------------------------------------------------");
                for (i = 0; i < file_list.Length; i++)
                {
                    Console.WriteLine("{2}\t\t{0}\t\t{1}",file_list[i].filename, file_list[i].filesize, i+1);
                }
                //다운로드/업로드 입력 받기
                Console.Write("메뉴선택 다운로드(번호입력) 업로드(0입력) 종료(음수입력) : ");
                i = int.Parse(Console.ReadLine());
                //조건문 -입력에 따른 코드 처리
                if (i > 0) // 다운로드
                {
                    //서버에 사용자 입력을 송신
                    formatter.Serialize(net_stream, i);
                    //사용자관점의 번호 -> 인덱스 번호로 변경
                    i--;
                    //파일 생성
                    file_stream = File.Create("./"+file_list[i].filename);
                    //반복 - 파일 사이즈 / 1024만큼 반복
                    for( int j = 0; j < file_list[i].filesize/1024f; j++)
                    {
                        //네트워크의 데이터 수신 및 수신데이터를 파일에 쓰기
                        data_size = net_stream.Read(datas, 0, datas.Length);
                        file_stream.Write(datas, 0, data_size);
                    }
                    //파일 종료
                    file_stream.Close();
                    Console.WriteLine("{0} 다운로드 완료", file_list[i].filename);

                }else if (i == 0) //업로드
                {
                    string path = "";
                    //업로드할 파일의 경로를 사용자 입력 받음
                    do
                    {
                        Console.Write("업로드할 파일을 입력 (확장자명포함): ");
                        path = Console.ReadLine();
                        //파일열기
                        try {
                            file_stream = File.OpenRead(path);
                            break;
                        }
                        catch //사용자가 입력한 문자열을 통해 파일을 찾을 수 없는 경우
                        {
                            Console.WriteLine("존재하지않는 파일입니다.");
                        }
                    } while (true);
                    File_info new_info = new File_info();
                    //문자열.Split() : 매개변수로 들어간 문자열 단위로 분리시키는 정적함수
                    //파일경로에서 \\마지막 부분에 파일 이름, 확자자명, 문자열이 있으므로Split함수에서 반환되는 문자열의 마지막 인덱스 값을 파일 
                    string[] splitstr = file_stream.Name.Split('\\');
                    string filename = splitstr[splitstr.Length - 1];

                    new_info.filename = filename;
                    new_info.filesize = file_stream.Length;
                    //사용자입력을 송신
                    formatter.Serialize(net_stream, i);
                    //File_info 객체 송신
                    formatter.Serialize(net_stream, new_info);
                    //업로드할 파일데이터 송신
                    for(i = 0; i < new_info.filesize / 1024f; i++)
                    {
                        data_size = file_stream.Read(datas, 0, datas.Length);
                        net_stream.Write(datas, 0, data_size);
                    }
                    file_stream.Close();
                }
                else //연결종료
                {
                    formatter.Serialize(net_stream, i);
                    break;
                }
            }
            net_stream.Close();
            client.Close();
        }
    }
}