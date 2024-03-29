﻿using data_struct;
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
            List<File_info> file_list = new List<File_info>();
            DirectoryInfo info = new DirectoryInfo("./files");
            //DirectoryInfo : 특정 폴더안에 있는 파일을 접근/ 폴더 생성, 이동 할때 사용하는 클래스
            //GetFiles : 접근한 폴더안에 있는 파일정보들을 추출하는 메소드

            //var : 프로그램이 시작하기전 해당 변수에 대입된 자료형/클래스를 보고 자동으로 자료형/클래스 맞게 변수를 선언해주는 문법
            //foreach (var fileInfo in info.GetFiles())
            //C# 개발자가 만든 Fileinfo 클래스에서 마일명/ 사이즈를 추출 및 저장
            foreach (FileInfo fileInfo in info.GetFiles())
            {
                File_info file_Info = new File_info();
                file_Info.filename = fileInfo.Name;
                file_Info.filesize = fileInfo.Length;
                file_list.Add(file_Info);
            }

            //서버 생성 - 10000번 포트 사용
            TcpListener listener = new TcpListener(11000);
            //클라이언트 연결 허용
            listener.Start();
            TcpClient client;
            NetworkStream net_stream;
            FileStream file_stream;
            byte[] datas = new byte[1024];
            int data_size;
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
            //무한반복 - 여러 클라이언트 접속 처리용도
            while (true)
            {
                //클라이언트 연결
                client = listener.AcceptTcpClient();
                net_stream = client.GetStream();
                //반복 - 클라이언트가 연결을 끊을때까지
                for(; ; )
                {
                    //파일목록을 저장한 변수를 클라이언트에게 송신
                    //List 객체를 네트워크로 송신할 수 없기 때문에
                    //List 객체를 배열로 바꿔 송신함
                    formatter.Serialize(net_stream, file_list.ToArray());
                    //클라이언트가 보낸 데이터 수신 - 기능선택
                    int select = (int)formatter.Deserialize(net_stream);
                    //조건문 - 클라이언트가 요청한 기능을 처리
                    if (select>0)//다운로드 요청
                    {
                        //사용자관점의 번호 -> 인덱스번호
                        select--;
                        //해당 인덱스에 있는 파일정보를 통해 파일 읽기 모드로 접근
                        file_stream = File.OpenRead("./files/" + file_list[select].filename);
                        //파일사이즈를 1024로 나눈만큼 반복
                        for (int i = 0; i < file_stream.Length / 1024f; i++)
                        {
                            //파일 읽기 및 네트워크에 송신
                            data_size = file_stream.Read(datas,0,datas.Length);
                            net_stream.Write(datas, 0, data_size);
                            
                        }
                        //파일닫기
                        file_stream.Close();

                    }else if(select == 0)//업로드 요청
                    {
                        //업로드할 파일의 정보를 수신 - File_info 객체
                        File_info new_fileinfo = (File_info)formatter.Deserialize(net_stream);
                        //file_list 객체에 수신된 객체 추가
                        file_list.Add(new_fileinfo);
                        //수신된 객체에 저장된 파일 이름으로 파일 생성
                        file_stream = File.Create("./files/" + new_fileinfo.filename);
                        //클라이언트가 보낸 파일데이터를 수신 및 파일에 쓰기
                        for (int i = 0; i < new_fileinfo.filesize / 1024f; i++)
                        {
                            data_size = net_stream.Read(datas,0, datas.Length);
                            file_stream.Write(datas, 0, data_size);
                        }
                        //파일 접근 종료
                        file_stream.Close();
                    }
                    else//연결종료
                    {
                        break;
                    }
                }
                net_stream.Close();
                client.Close();
            }
            listener.Stop();
        }


    }

}