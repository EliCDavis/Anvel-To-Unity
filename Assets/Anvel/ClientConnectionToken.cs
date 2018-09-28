using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CAVS.Anvel
{

    public class ClientConnectionToken 
    {
		string ipAddress;
		
		int port = 9094;

		public ClientConnectionToken(string ipAddress, int port){
			this.ipAddress = ipAddress;
			this.port = port;
		}

		public ClientConnectionToken(string ipAddress): this(ipAddress, 9094){
		}

		public ClientConnectionToken(): this("127.0.0.1", 9094){
		}

		public string GetIpAddress(){
			return this.ipAddress;
		}

		public int GetPort(){
			return this.port;
		}

    }

}