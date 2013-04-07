/// @file UnitySocket.cs
/// @brief Details to be specified
/// @author FvNano/LBT team
/// @author Marc Baaden <baaden@smplinux.de>
/// @date   2013-4
///
/// Copyright Centre National de la Recherche Scientifique (CNRS)
///
/// contributors :
/// FvNano/LBT team, 2010-13
/// Marc Baaden, 2010-13
///
/// baaden@smplinux.de
/// http://www.baaden.ibpc.fr
///
/// This software is a computer program based on the Unity3D game engine.
/// It is part of UnityMol, a general framework whose purpose is to provide
/// a prototype for developing molecular graphics and scientific
/// visualisation applications. More details about UnityMol are provided at
/// the following URL: "http://unitymol.sourceforge.net". Parts of this
/// source code are heavily inspired from the advice provided on the Unity3D
/// forums and the Internet.
///
/// This software is governed by the CeCILL-C license under French law and
/// abiding by the rules of distribution of free software. You can use,
/// modify and/or redistribute the software under the terms of the CeCILL-C
/// license as circulated by CEA, CNRS and INRIA at the following URL:
/// "http://www.cecill.info".
/// 
/// As a counterpart to the access to the source code and rights to copy, 
/// modify and redistribute granted by the license, users are provided only 
/// with a limited warranty and the software's author, the holder of the 
/// economic rights, and the successive licensors have only limited 
/// liability.
///
/// In this respect, the user's attention is drawn to the risks associated 
/// with loading, using, modifying and/or developing or reproducing the 
/// software by the user in light of its specific status of free software, 
/// that may mean that it is complicated to manipulate, and that also 
/// therefore means that it is reserved for developers and experienced 
/// professionals having in-depth computer knowledge. Users are therefore 
/// encouraged to load and test the software's suitability as regards their 
/// requirements in conditions enabling the security of their systems and/or 
/// data to be ensured and, more generally, to use and operate it in the 
/// same conditions as regards security.
///
/// The fact that you are presently reading this means that you have had 
/// knowledge of the CeCILL-C license and that you accept its terms.
///
/// $Id: UnitySocket.cs 225 2013-04-07 14:21:34Z baaden $
///
/// References : 
/// If you use this code, please cite the following reference : 	
/// Z. Lv, A. Tek, F. Da Silva, C. Empereur-mot, M. Chavent and M. Baaden:
/// "Game on, Science - how video game technology may help biologists tackle
/// visualization challenges" (2013), PLoS ONE 8(3):e57990.
/// doi:10.1371/journal.pone.0057990
///
/// If you use the HyperBalls visualization metaphor, please also cite the
/// following reference : M. Chavent, A. Vanel, A. Tek, B. Levy, S. Robert,
/// B. Raffin and M. Baaden: "GPU-accelerated atom and dynamic bond visualization
/// using HyperBalls, a unified algorithm for balls, sticks and hyperboloids",
/// J. Comput. Chem., 2011, 32, 2924
///

namespace SocketConnect.UnitySocket
{
   

  	using UnityEngine;
	using System;
	using System.Net.Sockets;
	using System.Net;
	using System.Collections;
	using System.Text;
	using Convert;
	using Cmd;
	using UI;
	
    public class UnitySocket
    {
		
		
        public static Socket mSocket = null;
      
        public UnitySocket()
        {
            
        }
		
		
		public static void SocketConnection(string LocalIP, int LocalPort)
		{
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		            try
		            {
						
		                IPAddress ip = IPAddress.Parse(LocalIP);
		                IPEndPoint ipe = new IPEndPoint(ip, LocalPort);
		              
				mSocket.Connect(ipe);
				
//				Send("<policy-file-request/>");//the first connect.
				string s="";
				System.Random radom= new System.Random();
				int radomKey=radom.Next(100000,999999);
				Debug.Log(radomKey);
				string strKey=radomKey.ToString();
				
//				while(s!="NC")
//				{
//					Send("<!policy-file-request>");//the second connect.if it's not equals policy head,it will come in the main connect!
//					s=ReceiveString(2);
//					Debug.Log(s);
//				}	
//				s="";			
//				while(s!="NC")
//				{
					Send(CommandID.LOGIN);
//					s=ReceiveString(2);
//					Debug.Log(s);
//				}
//				
//				Debug.Log("ibpc"+strKey);
//				while(s!="NC")
//				{
					Send("ibpc"+strKey);
					s=ReceiveString(2);
//				}
					Debug.Log(s);

				
//				string s=ReceiveString(2);
				if(s.Equals("OK"))
				{
					UIData.loginSucess=true;
				}
				
				else
				{
					UIData.loginSucess=false;
				}
				//Debug.Log(s.Length);
				Debug.Log(s);
		            }
		            catch (Exception e)
		            {
		                // Something went wrong, so lets get information about it.
						Debug.Log(e.ToString());
						UIData.loginSucess=false;
		            }
		}
		
		public static void Send(short data)
		{
			byte[] longth=TypeConvert.getBytes(data,false);
			mSocket.Send(longth);
			
		}
		
		public static void Send(long data)
		{
			byte[] longth=TypeConvert.getBytes(data,false);
			mSocket.Send(longth);
			
		}
		
		public static void Send(int data)
		{
			byte[] longth=TypeConvert.getBytes(data,false);
			mSocket.Send(longth);
			
		}
		
		public static void Send(string data)
		{
			byte[] longth=Encoding.UTF8.GetBytes(data);
			mSocket.Send(longth);
			
		}
		
		public static short ReceiveShort()
		{
			 byte[] recvBytes = new byte[2];
             mSocket.Receive(recvBytes,2,0);
			 short data=TypeConvert.getShort(recvBytes,true);
			 return data;
		}
		
		public static int ReceiveInt()
		{
			 byte[] recvBytes = new byte[4];
             mSocket.Receive(recvBytes,4,0);
			 int data=TypeConvert.getInt(recvBytes,true);
			 return data;
		}
		
		public static long ReceiveLong()
		{
			 byte[] recvBytes = new byte[8];
             mSocket.Receive(recvBytes,8,0);
			 long data=TypeConvert.getLong(recvBytes,true);
			return data;
		}
		
		public static string ReceiveString(int length)
		{
			 byte[] recvBytes = new byte[length];
             mSocket.Receive(recvBytes,length,0);
			 string data=Encoding.UTF8.GetString(recvBytes);
			 return data;
		}
		
		
	}
}
