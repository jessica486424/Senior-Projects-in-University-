using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace WindowsFormsApplication17
{
    class vowel
    {   
        //FILE    *in,  *out;
        FileStream fs;
        int     framesize, frameshift, bitspersample, numchannels, samplenumber;
        float[]   sound=new float[160000];
        double[]  logEn=new double[1600];
        double	logEncount=0.0;
        double[,] fourier=new double[100,56];//存第一次傅立葉的值
        double[,,] xxx=new double[50,56,10];//存第二次傅立葉的值  未知 
        float	ltime;
        double[]energy=new double[1600];
        double[]zcrarray=new double[1600];
        int a = 0;
        int[] start=new int[10];//儲存有聲音的開始點 
        double[] logEn2=new double[1600];//log(energy) 
        double[] acfSum=new double[1600];
        //unsigned char bitmap[50][56];
        int width=50, height=56;
        public int pinflag = 0, iaoflag = 0, liuflag = 0, fanflag = 0;



        public vowel()
        {
        }

        public String done(string filename)
        {
            	char[] infile=new char[40]; 
                char[] outfilename=new char[40];
                byte[] b = new byte[44];
	            //printf("\nEnter output file name:");
                //scanf("%s",outfilename);

	            int     i, j, bytespersample, samplerate, datasize, samplenumber,
                        totalframes, wsize, frameloc, location, windowsize;
                float   fsize_ms, fshift_ms, acfloc, maxmodu, minmodu, step, gvalue; 
                float wtime, stime;//stime = window shifting
                char[]  header1=new char[44];
                /*for (int i = 0; i<10; i++){
		            for(int s=0;s<40;s++){
			            for(int t=0;t<50;t++){
				            xxx[t][s][i]=0.0;
			            }	
		            }
	            }*/
	
	            //out=fopen(outfilename,"w");

	            a = 0;//!!!!!!!!我們從0改為10 
                //printf("Enter input wave file name <eg. xxxx.wav>:");
                //scanf("%s",infile);
	            fsize_ms=20;			
	            fshift_ms=10;
	
	            fs = new FileStream(@filename, FileMode.Open);
                fs.Read(b, 0,44);
                for (int tt = 0; tt < 44; tt++)
                    header1[tt] = Convert.ToChar(b[tt]);
                //in=fopen(infile,"rb");
   	            //fread(header1,1,44,in);
   	            numchannels=header1[23]*256+header1[22];
   	            //printf("\nNumber of channals:%d",numchannels);
   	            samplerate=header1[26]*256*256+header1[25]*256+header1[24];
   	            //printf("\nSampling rate:%d",samplerate);
                bitspersample=header1[35]*256+header1[34];
                //printf("\nBits per sample:%d",bitspersample);
                bytespersample=bitspersample/8;
   	            datasize=header1[7]*256*256*256+header1[6]*256*256
                         +header1[5]*256+header1[4]-36;
                samplenumber=datasize/(numchannels*bytespersample);  // number of samples
                if (samplenumber>160000) {
                    samplenumber=160000;
                    //printf("\nSample number reduced to 160000.");
                }
                //printf("\nTotal number of samples: %d",samplenumber);
                read_sound_file(samplenumber);
               
    
                framesize=(int)(samplerate*fsize_ms/1000);
                frameshift=(int)(samplerate*fshift_ms/1000); 
                totalframes=(samplenumber-framesize)/frameshift;
                //printf("\nTotal number of frames: %d",totalframes);
    
                //printf("\n Doing short-time energy ...");
                st_energy(totalframes);
                //printf("done.");
                //printf("\n Doing short-time zerocrossing ...");
                st_zerocrossing(totalframes);
               // printf("done.");
                //cout<<endl<<"zerocrossing"<<endl;
	            wsize=400;		//ACF window size
	
	            int x,framebegin;
	            //cout<<"每個音個第一高峰值(ACF)"<<endl;
	            for(x=0; x<totalframes; x++){				 
		            framebegin=x*frameshift;
		            acfSum[x]=acf(framebegin, wsize);		//每個音框的第一峰值
		            acfSum[x]=acfSum[x]*logEn[x];			// 
		            if(acfSum[x]!=0.0){
			            logEncount++;
		            }
	            }
	            //fprintf(out,"\n");
	            double acfTotalSum=0.0;
	            for(x=0; x<totalframes; x++){
		            acfTotalSum+=acfSum[x];					//ACF第一高峰值加總 
	            }
	            double average=0.0;
	            //cout<<acfTotalSum<<"  "<<logEncount<<endl;
	            average=acfTotalSum/logEncount;				//ACF第一高峰值的平均(週期): 男生100~200 女生50~100  
	            //cout<<average<<endl;
	            double acfTotalSum2=0.0; 
	            double sumCount=0.0;
	            double average2=0.0;						
	            for(x=0; x<totalframes; x++){				//過濾過的第一高峰值加總平均  
		            if(acfSum[x]>=(average-40)&&acfSum[x]<=(average+40)){		//平均值的上下40做過濾 
			            acfTotalSum2+=acfSum[x];
			            sumCount++;
		            }
	            }
	            average2=acfTotalSum2/sumCount;
	            //cout<<average2<<endl;
                //pitch(totalframes);			//高過零率的濾掉 
                enprint(totalframes);

   
	            wtime = 7;		 //length of window in ms
	            windowsize = (int)(wtime*samplerate / 1000);//音框包括多少個樣本 

	            int shiftwin;
	            stime = 1.5f;						 
	            shiftwin = (int)(stime*samplerate / 1000);//音框shift幾個樣本 
	            int numframe=100;					//傅立葉轉換   所需的音框數 

	            //做傅立葉轉換 
	            for (int ii = 0; ii<a; ii++){ //!!!!!!!!!!!!因為這裡如果a=0 迴圈會無法執行 導致 傅立葉根本沒有執行到 
		            location = (int)(start[ii]*frameshift);	//start[i] 過濾後找到可使用的點 
	 	            for (int count = 0; count < 100; count++){
		              spectrum(location, windowsize, count);
		              location = location + shiftwin;
		             }
		
	 	            //fprintf(out,"%10.4d    ",i);
	 	            //fprintf(out,"\n");
	 	            for (int n = 0; n < 56; n++){
	 	             spectrumII(numframe, n, ii);	 			//第二次傅立葉轉換 
		             }
		
		            //  fprintf(out,"\n");		
		            //fprintf(out,"\n\n");
		 

	            // fprintf(out,"\n\n");

            }
                //fclose(out);
                //cout<<"傅立葉done"<<endl; 
                //比對是誰的聲音 
	            /*for(int d=0;d<50;d++){
  	            for(int h=0;h<56;h++){
  		             cout<<xxx[d][h][0];
              }
              }*/
             
                /*
                StreamReader sp = new StreamReader("pin.txt");
                String[] strs = sp.ReadToEnd().Split(' ');

                foreach(string s in strs)
                {
                    Console.Write(s);
                }
                */
                int index_=0;
                double[] d_p = new double[50 * 56 * 10];
                using (TextReader reader = File.OpenText("pin.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] bits = line.Split(' ');
                        foreach (string bit in bits)
                        {
                            double value;
                            if (!double.TryParse(bit, out value))
                            {
                                
                            }
                            else
                            {
                               
                                d_p[index_] = value;
                                index_++;
                            }
                        }
                    }
                }
              
                index_ = 0;
                double[] d_i = new double[50 * 56 * 10];
                using (TextReader reader = File.OpenText("iao.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] bits = line.Split(' ');
                        foreach (string bit in bits)
                        {
                            double value;
                            if (!double.TryParse(bit, out value))
                            {

                            }
                            else
                            {
                                
                                d_i[index_] = value;
                                index_++;
                            }
                        }
                    }
                }
           
                index_ = 0;
                double[] d_1 = new double[50 * 56 * 10];
                using (TextReader reader = File.OpenText("13.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] bits = line.Split(' ');
                        foreach (string bit in bits)
                        {
                            double value;
                            if (!double.TryParse(bit, out value))
                            {

                            }
                            else
                            {
                                d_1[index_] = value;
                                index_++;
                            }
                        }
                    }
                }
             
                index_ = 0;
                double[] d_f = new double[50 * 56 * 10];
                using (TextReader reader = File.OpenText("fan.txt"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] bits = line.Split(' ');
                        foreach (string bit in bits)
                        {
                            double value;
                            if (!double.TryParse(bit, out value))
                            {

                            }
                            else
                            {
                                
                                d_f[index_] = value;
                                index_++;
                            }
                        }
                    }
                }
             
                //ifstream fp("pin.txt", ios::in);
	            //ifstream fi("iao.txt", ios::in);
	            //ifstream fl("13.txt", ios::in);
	            //ifstream ff("fan.txt", ios::in);
	            //ifstream fh("hu09.txt", ios::in);
	            //ifstream fg("logan09.txt", ios::in);
	            //ifstream fw("wei09.txt", ios::in);


	            //double xxx[50][56][10];   //測試者之矩陣 

	            double[,,] pin=new double[50,56,10];
	            double[,,] iao=new double[50,56,10];
	            double[,,] liu=new double[50,56,10];
	            double[,,] fan=new double[50,56,10];
	            //double hu[50][56][10];
	            //double gan[50][56][10];
	            //double wei[50][56][10];

	            double[] rp=new double[10];
	            double[] ri=new double[10];
	            double[] rl=new double[10];
	            double[] rf=new double[10];
	            //double rh[10];
	            //double rg[10];
	            //double rw[10];
                byte[] by=new byte[8];


                index_ = 0;
	            //cout<<"P"<<endl;
	            for (int ii = 0; ii<10; ii++){
		            for (int jj = 0; jj<56; jj++){
			            for (int kk = 0; kk<50; kk++){

                            
                            pin[kk, jj, ii] = (double)d_p[index_++];
                            
                               
			            }
		            }
	            }
               
                //foreach (double d in pin)
                    //Console.WriteLine(d);
                index_ = 0;
	            //cout<<"P1"<<endl;
	            for (int jj = 0; jj<10; jj++){
		            for (int ii = 0; ii<56; ii++){
			            for (int kk = 0; kk<50; kk++){
                           
                            iao[kk, ii, jj] = d_i[index_++];
                            
			            }
		            }
	            }
                index_ = 0;
	            //cout<<"P2"<<endl;
	            for (int bb = 0; bb<10; bb++){
		            for (int ii = 0; ii<56; ii++){
			            for (int kk = 0; kk<50; kk++){

                            liu[kk, ii, bb] = d_1[index_++];
				           
			            }
		            }
	            }
                index_ = 0;
	            //cout<<"P3"<<endl;
	            for (int bb = 0; bb<10; bb++){
		            for (int ii = 0; ii<56; ii++){
			            for (int kk = 0; kk<50; kk++){

                            fan[kk, ii, bb] = d_f[index_++];
                           
			            }
		            }
	            }
	            
	            for (int ii = 0; ii<10; ii++){
		            for (int ss = 0; ss<56; ss++){
			            for (int tt = 0; tt<50; tt++){
				            xxx[tt,ss,ii] = xxx[tt,ss,ii] - 8.0f;//受試者 
			            }
		            }
	            }
	            //cout<<"Relation計算開始"<<endl; 
	
	            relation(pin, rp);
	            relation(iao, ri);
	            relation(liu, rl);
	            relation(fan, rf);
	
	            //cout<<"相關係數Done"<<endl; 
	            //比對相關係數 最後印出結果
	
	           

	            for(int ii=0;ii<=6;ii++)
	            {
                    Console.WriteLine(rp[ii].ToString() + " " + ri[ii].ToString() + " " + rl[ii].ToString() + " " + rf[ii].ToString());
                    if (rp[ii] >= ri[ii] && rp[ii] >= rl[ii] && rp[ii] >= rf[ii])
		            {
			            pinflag++;
		            }
		            else if(ri[ii]>=rp[ii] && ri[ii]>=rl[ii] && ri[ii]>=rf[ii])
		            {
			            iaoflag++;
		            }
		            else if(rl[ii]>=ri[ii] && rl[ii]>=rp[ii] && rl[ii]>=rf[ii])
		            {
			            liuflag++;
		            }
		            else if(rf[ii]>=ri[ii] && rf[ii]>=rl[ii] && rf[ii]>=rp[ii])
		            {
			            fanflag++;
		            }
		           
	            }
                fs.Close();
                
                Console.Write(pinflag + " " + iaoflag + " " + liuflag + " " + fanflag);
                if ((pinflag > iaoflag) && (pinflag > liuflag) && pinflag > fanflag)
                {
                    return "鄧同學";
                    //cout << "鄧同學" << endl;
                }
                if (iaoflag > pinflag && iaoflag > liuflag && iaoflag > fanflag)
                {
                    return "尤同學";
                    //cout << "尤同學" << endl;
                }
                if (liuflag > iaoflag && liuflag > pinflag && liuflag > fanflag)
                {
                    return "莊同學";
                    //cout << "莊同學" << endl;
                }
                if (fanflag > iaoflag && fanflag > liuflag && fanflag > pinflag)
                {
                    return "范同學";
                    //cout << "范同學" << endl;
                }

                return "都不相似";
        }

        void read_sound_file(int number)
        {
            int i, bytespersample;
            short datashort;
            double sigsum, bias;
            byte[] b = new byte[2];
            bytespersample = bitspersample / 8;
            if (bytespersample == 2)
            {                    // regular
                if (numchannels == 1)
                {                   // mono
                    for (i = 0; i < number; i++)
                    {
                        //fread(&datashort,2,1,in);
                        fs.Read(b, 0, 2);
                        datashort = BitConverter.ToInt16(b, 0);
                 
                        sound[i] = (float)datashort / (float)32768.0;
                    }
                }
                else
                {                               // stereo to mono
                    for (i = 0; i < number; i++)
                    {
                        fs.Read(b, 0, 2);
                      
                        datashort = BitConverter.ToInt16(b, 0);
                        sound[i] = (float)datashort / (float)32768.0;
                        fs.Read(b, 0, 2);
               
                        datashort = BitConverter.ToInt16(b, 0);
                        sound[i] = (sound[i] + (float)datashort / (float)32768.0) / (float)2.0;
                    }
                }
            }
            sigsum = 0.0;
            for (i = 0; i < number; i++) sigsum = sigsum + sound[i];
            bias = sigsum / (float)number;
            for (i = 0; i < number; i++) sound[i] = sound[i] - (float)bias;
        }
        void st_energy(int totalframes)
        {
             int     i, j, framestart;
     
             for (i=0; i<totalframes; i++) {
                 energy[i]=0.0;
                 framestart=i*frameshift;
                 for (j=framestart; j<framestart+framesize; j++) {
                     energy[i]=energy[i]+sound[j]*sound[j];
		        }
                 logEn[i] = 10.0 * Math.Log10(energy[i] + 0.000001);
		         logEn2[i]=logEn[i];
			         if(logEn[i]<=-13.0){		//log(energy)的閥值 -13.0 (原本觀察出的閥值)
				         logEn[i]=0.0;
			         }
			        else{ 
				         logEn[i]=1.0;
				 
			         }
             }
            
	        
	      
             for (i=0; i<totalframes; i++) {
         
     	        energy[i]=energy[i]*logEn[i];
	         }
	     
	     
        }
        void st_zerocrossing(int totalframes)
        {
            int i, j, framestart, zccount;
            double zcr;

            for (i = 0; i < totalframes; i++)
            {
                zccount = 0;
                framestart = i * frameshift;
                for (j = framestart; j < framestart + framesize; j++)
                {
                    if (sound[j] * sound[j + 1] < 0.0) zccount++;
                }
                zcr = (float)zccount / (float)framesize;
                zcrarray[i] = zcr;
                //  fprintf(out,"%10.4f    ", zcr);			//zerocrossing的值 
            }
            //fprintf(out,"\n");
        } 

        float[]    ss=new float[512];
        float[, ]  realm=new float[512,256];
        float[,]   imagm=new float[512,256];
        float[]    win = new float[512];
        void spectrum(int loc, int npoints, int a)
        {
	
	         int           i, j, k, np12;
	         double        w, xx, yy, mag, step, x, y;
	         double[] db=new double[256];
	         if (npoints>512) npoints = 512;  //最多做512點的DFT 
	         np12 = npoints / 2;
	         for (j = 0; j<256; j++) db[j] = 0.0;
	
	         //--------- generate transformation matrices and window function --------
	
	         w = 2.0*Math.PI / (float)npoints;
	         for (i = 0; i<npoints; i++) {
	          for (j = 0; j<np12; j++) {
	           realm[i,j] = (float)Math.Cos(w*i*j);
	           imagm[i,j] = (float)Math.Sin(w*i*j);
	          }
	         }
		
	         for (i = 0; i<npoints; i++)
	          win[i] = (float)(0.54 - 0.46*Math.Cos((2.0*Math.PI*i)) / (float)(npoints - 1));  //Hamming window
	
	         //---------- high freg emphasis -------------------
	
	         for (i = 0; i<npoints; i++) ss[i] = 10.0f*sound[loc + i] - 9.5f*sound[loc + i - 1];		//高頻增強 
	         for (i = 0; i<npoints; i++) {
	          ss[i] = ss[i] * win[i];           // windowing
	         }
	         for (i = 0; i<np12; i++) {
	          xx = 0.0;
	          yy = 0.0;
	          for (k = 0; k<npoints; k++) {
	           xx = xx + ss[k] * realm[k,i];
	           yy = yy - ss[k] * imagm[k,i];
	          }
	          mag = Math.Sqrt(xx*xx + yy*yy);
	          fourier[a,i] = (float)(10.0*Math.Log10(mag + 0.000001)); //取對數
	  
	          fourier[a,i] += 15.0;//第一次 Fourier +15 
	         }

        }
    
        float[]    ssII=new float[512];
        float[, ]  realmII=new float[512,256];
        float[,]   imagmII=new float[512,256];
        float[]    winII = new float[512];

        void spectrumII(int npoints, int n, int A)
        {
        //	cout<<"第二次完成01";
         int           i, j, k, np12;
         double[] db=new double[256];
         double        w, xx, yy, mag, step, x, y;

         //if (npoints>512) npoints = 512;  //最多做512點的DFT 
         np12 = npoints / 2;
         for (j = 0; j<256; j++) db[j] = 0.0;

         //--------- generate transformation matrices and window function --------

         w = 2.0*Math.PI / (float)npoints;
	         for (i = 0; i<npoints; i++) {
	          for (j = 0; j<np12; j++) {
	           realmII[i,j] = (float)Math.Cos(w*i*j);
	           imagmII[i,j] = (float)Math.Sin(w*i*j);
	          }
	         }

         for (i = 0; i<npoints; i++)
            winII[i] = (float)(0.54 - 0.46*Math.Cos((2.0*Math.PI*i)) / (float)(npoints - 1));  //Hamming window

         for (i = 0; i<npoints; i++) {
          ssII[i] = (float)fourier[i,n];			//將第一次傅立葉的值拿出來存放到 ss[i] 
          ssII[i] = ssII[i] * winII[i];           // windowing
         }
         for (i = 0; i<np12; i++) {
          xx = 0.0;
          yy = 0.0;
          for (k = 0; k<npoints; k++) {
           xx = xx + ssII[k] * realmII[k,i];
           yy = yy - ssII[k] * imagmII[k,i];
          }
          mag = Math.Sqrt(xx*xx + yy*yy);
          db[i] = (float)mag;				//第二次傅立葉值 
          db[i] = 10*Math.Log10(db[i]+0.000001); 		//取對數 
          xxx[i,n,A]=db[i];				//fourier2[50][56][10][2]
          //fprintf(out, "%10.4f    ", db[i]);
         }

         //fprintf(out, "\n");
 
        }
        void enprint(int totalframes)
        {
            //cout<<"濾過的log(energy)"<<endl;
            for (int i = 0; i < totalframes; i++)
            {
                if (logEn2[i] <= -5.0)
                {				//log(energy) -5.0為老師所要求的閥值 
                    logEn2[i] = -30.0;
                    acfSum[i] = 0.0;
                }
                //fprintf(out,"%10.4f    ", logEn2[i]);

            }
            //fprintf(out,"\n");
            //cout<<"過濾的ACF"<<endl;
            for (int i = 0; i < totalframes; i++)
            {
                // fprintf(out,"%10.4f    ", acfSum[i]);	//用log(energy)過濾後的ACF 
            }

            // fprintf(out,"\n");
            int begin, end=0;
            //cout<<"有聲音的點"<<endl;
            for (int i = 0; i < totalframes; i++)
            {
                if (acfSum[i] > 0.0)
                {					//找有聲音的區間 
                    begin = i;							//當ACF不等於零時 為聲音的起點 
                    //printf("%d\n", begin);
                    for (int j = begin; j < totalframes; j++)
                    {
                        if (acfSum[j] == 0.0)
                        {			//當ACF回到零時 為終點 
                            end = j;
                            //printf("%d\n", end);
                            i = j;
                            break;
                        }

                    }
                    if (end - begin < 10)
                    {				//起點與終點的間距若小於10個樣本數時  
                        for (int k = begin; k < end; k++)
                        {
                            logEn2[k] = -30.0;			//將此區間之log(energy)設為-30.0 
                        }
                    }
                    else
                    {
                        start[a] = begin + 2;				// 將有聲音的起點存入start[a](全域)   (往後移2個) 

                        //	fprintf(out, "%10d    ", start[a]);

                        a++;
                    }

                }


            }
            //	fprintf(out,"\n");
            //cout<<"去掉過短雜音的log(energy)"<<endl;
            for (int i = 0; i < totalframes; i++)
            {
                //  fprintf(out,"%10.4f    ", logEn2[i]);	//log(energy)去掉屑屑 
            }
            // fprintf(out,"\n");


        }
        double acf(int fstart, int width)
        {
             int      i, j, k,d=0;
             double   sum, ss1, acf=0;
	         float[] acf1=new float[405],com=new float[100];

     
             ss1=0.0;
             for (i=fstart; i<fstart+width; i++) ss1=ss1+sound[i]*sound[i];    //做分母 
             for (k=0; k<width; k++) {
                 sum=0.0;
                 for (i=k; i<width; i++) {
                     sum=sum+sound[i+fstart]*sound[i+fstart-k];
                 }
                 acf=sum/ss1;
		         acf1[k]=(float)acf;         
             }

	         float temp = 0.0f;						//暫存峰值(不一定為最高峰) 
	         int max=0;
	         for(int m=51;m<width;m++)				//m=51 因為前面50個點過大 不使用 
	         {
		         if(acf1[m+1]<acf1[m] && acf1[m-1]<acf1[m] && acf1[m]>0.4){	//找峰值 
			
			        com[d]=acf1[m]; 
			         if (com[d]>temp){
				         temp = com[d];				//com[] compare
				         max=m;						//max存第一高峰的週期 
			         }

			        d++;
			        }
		 
	         }


	        // fprintf(out,"%10d    ",max);

	         return max;
        }
        void relation(double[, ,] pin, double[] rp)
        {
            

          
            double son = 0.0f;
            double mom = 0.0f;
            double first = 0.0f;
            double second = 0.0f;
            double totalx = 0.0f;
            double totaly = 0.0f;
            double totalz = 0.0f;

            for (int i = 0; i < 7; i++)
            {

                son = 0.0f;
                mom = 0.0f;
                first = 0.0f;
                second = 0.0f;
                totalx = 0.0f;
                totaly = 0.0f;
                totalz = 0.0f;

                for (int s = 0; s < 56; s++)
                {
                    for (int t = 0; t < 50; t++)
                    {
                        pin[t, s, i] = pin[t, s, i] - 8.0;//檔案原有的sample 
                        //	xxx[t][s][i] = xxx[t][s][i] - 8.0;//受試者 
                    }

                }
                
                for (int s = 0; s < 37; s++)
                {
                    for (int t = 0; t < 1; t++)
                    {
                        totalx += pin[t, s, i];		//xxx[50][56][10]
                        totaly += xxx[t, s, i];
                    }

                }
                
                totalx = totalx / (37);
                totaly = totaly / (37);
                
                for (int s = 0; s < 37; s++)
                {
                    for (int t = 0; t < 1; t++)
                    {		//第一條

                        son += (pin[t, s, i] - totalx) * (xxx[t, s, i] - totaly);
                        first += (pin[t, s, i] - totalx) * (pin[t, s, i] - totalx);
                        second += (xxx[t, s, i] - totaly) * (xxx[t, s, i] - totaly);

                    }
                }
              

                mom = Math.Sqrt(first) * Math.Sqrt(second);
                double fir = son / mom;
              
                //cout<<"---------------"<<endl;
                son = 0.0f;
                mom = 0.0f;
                first = 0.0f;
                second = 0.0f;
                totalx = 0.0f;
                totaly = 0.0f;

                for (int s = 0; s < 37; s++)
                {
                    for (int t = 2; t < 50; t++)
                    {
                        totalx += pin[t, s, i];
                        totaly += xxx[t, s, i];
                      
                    }

                }
                totalx = totalx / (37 * 48);
                totaly = totaly / (37 * 48);

                for (int s = 0; s < 37; s++)
                {
                    for (int t = 2; t < 50; t++)
                    {		//35*48 
                        son += pin[t, s, i] * xxx[t, s, i];
                        first += pin[t, s, i] * pin[t, s, i];
                        second += xxx[t, s, i] * xxx[t, s, i];
                    }
                }
                mom = Math.Sqrt(first) * Math.Sqrt(second);
                double th = son / mom;
                //cout<<"son"<<son<<"mon"<<mom<<endl;
                //Console.WriteLine(fir + " " + th + " "+(fir + th) / 2);
                rp[i] = (fir + th) / 2;

            }
        }



    }
}
