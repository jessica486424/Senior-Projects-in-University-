
/*----------------------------------------------------------------

                        speech time domain 1042.cpp


        Jia-Guu Leu      2016.4.4
----------------------------------------------------------------*/
#include <stdio.h>
#include <math.h>
#include <stdlib.h>
#include<iostream>
using namespace std;
#define pi 3.141592654

FILE    *in,  *out;
int     framesize, frameshift, bitspersample, numchannels, samplenumber;
float   sound[160000];
double	logEn[1600];
double	logEncount=0.0;
double fourier[100][56];//存第一次傅立葉的值
double fourier2[50][56][10];//存第二次傅立葉的值
float	ltime;
double	energy[1600];
double	zcrarray[1600];
int a = 0;
int start[10];//儲存有聲音的開始點 
double logEn2[1600];//log(energy) 
double acfSum[1600];
unsigned char bitmap[50][56];
int width=50, height=56;
int main()
{	
	char    infile[40], outfilename[40];

	printf("\nEnter output file name:");
    scanf("%s",outfilename);

	void    read_sound_file(int);
	void    st_energy(int);
	void    st_zerocrossing(int);
	double    acf(int, int);
	void    spectrum(int, int, int);
 	void    spectrumII(int, int, int);
 	void	pitch(int);
 	void	enprint(int);
 	int     writebmpfile(int,int);
	int     i, j, bytespersample, samplerate, datasize, samplenumber,
            totalframes, wsize, frameloc, location, windowsize;
    float   fsize_ms, fshift_ms, acfloc, maxmodu, minmodu, step, gvalue; 
     float wtime, stime;//stime = window shifting
    unsigned char header1[44];
    for (int i = 0; i<10; i++){
		for(int s=0;s<40;s++){
			for(int t=0;t<50;t++){
				fourier2[t][s][i]=0.0;
				
			}	
		}
	}
	//char    infile[40], outfilename[40];
	out=fopen(outfilename,"w");
//for(int b=0;b<2;b++){				//自動跑兩個檔 
	a = 0;
    printf("Enter input wave file name <eg. xxxx.wav>:");
    scanf("%s",infile);
    /*printf("\nEnter frame size in ms <eg. 20>:");
    scanf("%f",&fsize_ms);
    printf("\nEnter frame shift in ms <eg. 10>:");
    scanf("%f",&fshift_ms);*/
	fsize_ms=20;			
	fshift_ms=10;
	
	
    in=fopen(infile,"rb");
   	fread(header1,1,44,in);
   	numchannels=header1[23]*256+header1[22];
   	printf("\nNumber of channals:%d",numchannels);
   	samplerate=header1[26]*256*256+header1[25]*256+header1[24];
   	printf("\nSampling rate:%d",samplerate);
    bitspersample=header1[35]*256+header1[34];
    printf("\nBits per sample:%d",bitspersample);
    bytespersample=bitspersample/8;
   	datasize=header1[7]*256*256*256+header1[6]*256*256
             +header1[5]*256+header1[4]-36;
    samplenumber=datasize/(numchannels*bytespersample);  // number of samples
    if (samplenumber>160000) {
        samplenumber=160000;
        printf("\nSample number reduced to 160000.");
    }
    printf("\nTotal number of samples: %d",samplenumber);
    read_sound_file(samplenumber);
    fclose(in);
    
    framesize=(int)(samplerate*fsize_ms/1000);
    frameshift=(int)(samplerate*fshift_ms/1000); 
    totalframes=(samplenumber-framesize)/frameshift;
    printf("\nTotal number of frames: %d",totalframes);
    
    printf("\n Doing short-time energy ...");
    st_energy(totalframes);
    printf("done.");
    printf("\n Doing short-time zerocrossing ...");
    st_zerocrossing(totalframes);
    printf("done.");
    cout<<endl<<"zerocrossing"<<endl;
    /*printf("\nEnter ACF window size L <eg. 400>:");
    scanf("%d", &wsize);*/
	wsize=400;
	
	int x,framebegin;
	cout<<"每個音個第一高峰值(ACF)"<<endl;
	for(x=0; x<totalframes; x++){				 
		framebegin=x*frameshift;
		acfSum[x]=acf(framebegin, wsize);		//每個音框的第一峰值
		acfSum[x]=acfSum[x]*logEn[x];			// 
		if(acfSum[x]!=0.0){
			logEncount++;
		}
	}
	fprintf(out,"\n");
	double acfTotalSum=0.0;
	for(x=0; x<totalframes; x++){
		acfTotalSum+=acfSum[x];					//ACF第一高峰值加總 
	}
	double average=0.0;
	cout<<acfTotalSum<<"  "<<logEncount<<endl;
	average=acfTotalSum/logEncount;				//ACF第一高峰值的平均(週期): 男生100~200 女生50~100  
	cout<<average<<endl;
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
	cout<<average2<<endl;
    //pitch(totalframes);			//高過零率的濾掉 
    enprint(totalframes);

   //scanf("%f", &ltime);			//輸入 有聲音的點 
	 /*printf("\nEnter time length of window in ms (eg. 5):");
	 scanf("%f", &wtime);*/
	 wtime = 7;		 
	
	windowsize = (int)(wtime*samplerate / 1000);
	int shiftwin;
	stime = 1.5;						 
	shiftwin = (int)(stime*samplerate / 1000);//音框shift幾個樣本 
	int numframe;					//傅立葉轉換   所需的音框數 
	/*printf("\nEnter the number of frames (eg. 100):");
	scanf("%d", &numframe);*/
	numframe=100;
	
	for (int i = 0; i<a; i++){
		location = (int)(start[i]*frameshift);	//start[i] 過濾後找到可使用的點 
	 	for (int count = 0; count < 100; count++){
		  spectrum(location, windowsize, count);
		  location = location + shiftwin;
		 }
	
	 	//fprintf(out,"%10.4d    ",i);
	 	fprintf(out,"\n");
	 	for (int n = 0; n < 56; n++){
	 	 spectrumII(numframe, n, i);	 			//第二次傅立葉轉換 
		 }
		  fprintf(out,"\n");
		
		 /*for(int j=0;j<numframe/2;j++){
		 	double Sum=0.0;
		 	for(int k=0;k<windowsize/2;k++){
		 		Sum=Sum+fourier2[j][k][i][b];			//第二次傅立葉轉換後 縱軸加總 
			 }
			 fprintf(out,"%10.4f    ", Sum);
		 }*/
		 fprintf(out,"\n\n");
		 
		maxmodu=-9999.99;
    	minmodu=9999.99;
   		for (int k=0; k<50; k++) {
    		for (int j=0; j<56; j++) {
    			if (fourier2[k][j][i]>maxmodu) maxmodu=fourier2[k][j][i];
    			if (fourier2[k][j][i]<minmodu) minmodu=fourier2[k][j][i];
    		}	
		}
 	   step=(maxmodu-minmodu)/255.0;	
 	   for (int k=0; k<50; k++) {
   		 	for (int j=0; j<56; j++) {
   				gvalue=(fourier2[k][j][i]-minmodu)/step;
    			if (gvalue>255.0) gvalue=255.0;
    			if (gvalue<0.0) gvalue=0.0;
    			bitmap[k][j]=(unsigned char)gvalue;
			}
		}
		//writebmpfile(b,i);
	 }
	 fprintf(out,"\n\n");

//}
	
	
	
	

    fclose(out);
    system("PAUSE");
    return(0);
}

void read_sound_file(int number)
{
     int      i, bytespersample;
     short    datashort;
     double   sigsum,bias;

	for(int z=0;z<160000;z++){		//initializing
		sound[z]=0.0;
	}

    bytespersample=bitspersample/8;
    if (bytespersample==2) {                    // regular
        if (numchannels==1) {                   // mono
            for (i=0; i<number; i++) {
                fread(&datashort,2,1,in);
                sound[i]=(float)datashort/32768.0;
            }
        }
        else {                               // stereo to mono
            for (i=0; i<number; i++) {
                fread(&datashort,2,1,in);
                sound[i]=(float)datashort/32768.0;
                fread(&datashort,2,1,in);
                sound[i]=(sound[i]+(float)datashort/32768.0)/2.0;
            }
        }
    }
    sigsum=0.0;
    for (i=0; i<number; i++) sigsum=sigsum+sound[i];
    bias=sigsum/(float)number;
    for (i=0; i<number; i++) sound[i]=sound[i]-bias;
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
		 logEn[i]=10.0*log10(energy[i]+0.000001);
		 logEn2[i]=logEn[i];
			 if(logEn[i]<=-13.0){		//log(energy)的閥值 -13.0 (原本觀察出的閥值)
				 logEn[i]=0.0;
			 }
			else{ 
				 logEn[i]=1.0;
				 
			 }
     }
     cout<<"原始的energy "<<endl;
    for (i=0; i<totalframes; i++) {
    	//fprintf(out,"%10.4f    ", energy[i]);	//原始的energy 
    	
	 }
	fprintf(out,"\n");
	cout<<"利用log(energy)的閥值 -13.0 來做刪減後的 energy"<<endl;
     for (i=0; i<totalframes; i++) {
        // fprintf(out,"%10.4f    ", energy[i]*logEn[i]);//利用log(energy)的閥值 -13.0 來做刪減後的 energy
     	energy[i]=energy[i]*logEn[i];
	 }
	fprintf(out,"\n");
	cout<<"log(energy)的值"<<endl;
	 for (i=0; i<totalframes; i++) {
       //  fprintf(out,"%10.4f    ", logEn2[i]);		//log(energy)的值 
     }
     fprintf(out,"\n");
}
void st_zerocrossing(int totalframes)
{
     int     i, j, framestart, zccount;
     double  zcr;
	
     for (i=0; i<totalframes; i++) {
         zccount=0;
         framestart=i*frameshift;
         for (j=framestart; j<framestart+framesize; j++) {
             if (sound[j]*sound[j+1]<0.0) zccount++;
         }
         zcr=(float)zccount/(float)framesize;
		 zcrarray[i] = zcr;
        // fprintf(out,"%10.4f    ", zcr);			//zerocrossing的值 
     }
     fprintf(out,"\n");
} 
void spectrum(int loc, int npoints, int a)
{
 int           i, j, k, np12;
 float         ss[512], realm[512][256], imagm[512][256], win[512];
 double        w, xx, yy, mag, db[256], step, x, y;

 if (npoints>512) npoints = 512;  //最多做512點的DFT 
 np12 = npoints / 2;
 for (j = 0; j<256; j++) db[j] = 0.0;

 //--------- generate transformation matrices and window function --------

 w = 2.0*pi / (float)npoints;
 for (i = 0; i<npoints; i++) {
  for (j = 0; j<np12; j++) {
   realm[i][j] = cos(w*i*j);
   imagm[i][j] = sin(w*i*j);
  }
 }

 for (i = 0; i<npoints; i++)
  win[i] = 0.54 - 0.46*cos((2.0*pi*i) / float(npoints - 1));  //Hamming window

 //---------- high freg emphasis -------------------

 for (i = 0; i<npoints; i++) ss[i] = 10.0*sound[loc + i] - 9.5*sound[loc + i - 1];		//高頻增強 
 for (i = 0; i<npoints; i++) {
  ss[i] = ss[i] * win[i];           // windowing
 }
 for (i = 0; i<np12; i++) {
  xx = 0.0;
  yy = 0.0;
  for (k = 0; k<npoints; k++) {
   xx = xx + ss[k] * realm[k][i];
   yy = yy - ss[k] * imagm[k][i];
  }
  mag = sqrt(xx*xx + yy*yy);
  fourier[a][i] = (float)(10.0*log10(mag + 0.000001)); //取對數
  
  fourier[a][i] += 15.0;//第一次 Fourier +15 
 }

}

void spectrumII(int npoints, int n, int A )
{
 int           i, j, k, np12;
 float         ss[512], realm[512][256], imagm[512][256], win[512];
 double        w, xx, yy, mag, db[256], step, x, y;

 //if (npoints>512) npoints = 512;  //最多做512點的DFT 
 np12 = npoints / 2;
 for (j = 0; j<256; j++) db[j] = 0.0;

 //--------- generate transformation matrices and window function --------

 w = 2.0*pi / (float)npoints;
 for (i = 0; i<npoints; i++) {
  for (j = 0; j<np12; j++) {
   realm[i][j] = cos(w*i*j);
   imagm[i][j] = sin(w*i*j);
  }
 }

 for (i = 0; i<npoints; i++)
  win[i] = 0.54 - 0.46*cos((2.0*pi*i) / float(npoints - 1));  //Hamming window

 for (i = 0; i<npoints; i++) {
  ss[i] = fourier[i][n];			//將第一次傅立葉的值拿出來存放到 ss[i] 
  ss[i] = ss[i] * win[i];           // windowing
 }
 for (i = 0; i<np12; i++) {
  xx = 0.0;
  yy = 0.0;
  for (k = 0; k<npoints; k++) {
   xx = xx + ss[k] * realm[k][i];
   yy = yy - ss[k] * imagm[k][i];
  }
  mag = sqrt(xx*xx + yy*yy);
  db[i] = (float)mag;				//第二次傅立葉值 
  db[i] = 10*log10(db[i]+0.000001); 		//取對數 
  fourier2[i][n][A]=db[i];				//fourier2[50][56][10][2]
  fprintf(out, "%10.4f    ", db[i]);
 }

 fprintf(out, "\n");
}

void	enprint(int totalframes){
	cout<<"濾過的log(energy)"<<endl;
     for(int i=0; i<totalframes; i++){
     	if(logEn2[i]<=-5.0){				//log(energy) -5.0為老師所要求的閥值 
     		logEn2[i]=-30.0;
     		acfSum[i] = 0.0;
		 }
	//	 fprintf(out,"%10.4f    ", logEn2[i]);
     	 	
	 }
	 fprintf(out,"\n");
	 cout<<"過濾的ACF"<<endl;
	for(int i=0; i<totalframes; i++){
    // 	 fprintf(out,"%10.4f    ", acfSum[i]);	//用log(energy)過濾後的ACF 
	 }
	
	 fprintf(out,"\n");
	int begin, end;
	cout<<"有聲音的點"<<endl;
	for (int i = 0; i<totalframes; i++){
		if (acfSum[i]>0.0 ){					//找有聲音的區間 
			begin = i;							//當ACF不等於零時 為聲音的起點 
			printf("%d\n", begin);
			for (int j = begin; j<totalframes; j++){
				if (acfSum[j] == 0.0){			//當ACF回到零時 為終點 
					end = j;
					//printf("%d\n", end);
					i = j;
					break;
				}

			}
			if (end - begin<10){				//起點與終點的間距若小於10個樣本數時  
				for(int k=begin;k<end;k++){
					logEn2[k]=-30.0;			//將此區間之log(energy)設為-30.0 
				}
			}
			else{
				start[a] = begin+2;				// 將有聲音的起點存入start[a](全域)   (往後移2個) 
				
			//	fprintf(out, "%10d    ", start[a]);
				
				a++;
			}

		}
		
	 
	}
	fprintf(out,"\n");
	cout<<"去掉過短雜音的log(energy)"<<endl;
	for (int i=0; i<totalframes; i++) {
     //    fprintf(out,"%10.4f    ", logEn2[i]);	//log(energy)去掉屑屑 
     }
     fprintf(out,"\n");
     
   
}
double acf(int fstart, int width)
{
     int      i, j, k,d=0;
     double   sum, ss1, acf;
	 float acf1[405],com[100];

     
     ss1=0.0;
     for (i=fstart; i<fstart+width; i++) ss1=ss1+sound[i]*sound[i];    //做分母 
     for (k=0; k<width; k++) {
         sum=0.0;
         for (i=k; i<width; i++) {
             sum=sum+sound[i+fstart]*sound[i+fstart-k];
         }
         acf=sum/ss1;
		 acf1[k]=acf;         
     }

	 float temp = 0.0;						//暫存峰值(不一定為最高峰) 
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


	 //fprintf(out,"%10d    ",max);

	 return max;
}

int writebmpfile(int b,int a)
{					//b:第幾個檔 a:第幾個音 
	FILE   *out2;
	char   outfilename[40];
	unsigned char signature[3], line[4096], map[4];
	int  i, j, extra;
    struct bmpheader{
           long  filesize;
           long  reserved;
           long  dataoffset;
           long  headersize;
           long  width;
           long  height;
           short planes;
           short bitcount;
           long  compression;
           long  imagesize;
           long  h_res;
           long  v_res;
           long  colorsused;
           long  imptcolors;
           } header2;
	
    printf("Enter output BMP file name <eg. xxxx.bmp>:");
    scanf("%s",outfilename);
    /*outfilename[0]=(char)b;
    outfilename[1]=(char)a;
    outfilename[2]='.';
    outfilename[3]='b';
    outfilename[4]='m';
    outfilename[5]='p';*/
    out2=fopen(outfilename,"wb");
    signature[0]='B';
    signature[1]='M';
    if (width%4>0) extra=4-width%4; else extra=0;
    header2.filesize=54+1024+height*(width+extra);
    header2.reserved=0;
    header2.dataoffset=1078;
    header2.headersize=40;
    header2.width=width;
    header2.height=height;
    header2.planes=1;
    header2.bitcount=8;
    header2.compression=0;
    header2.imagesize=height*(width+extra);
    header2.h_res=3780;
    header2.v_res=3780;
    header2.colorsused=0;
    header2.imptcolors=0;
    fwrite(signature,1,2,out2);
   	fwrite(&header2,52,1,out2);
   	for (i=0; i<256; i++) {
        map[0]=map[1]=map[2]=i;
        map[3]=0;
   	    fwrite(map,4,1,out2);
    }
    for (j=0; j<height; j++) {
        for (i=0; i<width+extra; i++) line[i]=255;
        for (i=0; i<width; i++) {
            if (bitmap[i][j]>255) line[i]=0;
            else if (bitmap[i][j]<0)   line[i]=255;
            else line[i]=255-bitmap[i][j];
        }      
        fwrite(line,1,header2.width+extra,out2);
    }
    fclose(out2);
    return(0);
}
