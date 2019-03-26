using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace WindowsFormsApplication17
{
    public partial class Form1 : Form
    {
        FileStream fs;
        String filename;
        int     framesize, frameshift, bitspersample, numchannels;
        float[]     sound=new float[160000];
        double[]	logEn=new double[1600];
        double[]	logEn2=new double[1600];
        double  	logEncount=0.0;
        double[]	zcrarray=new double[1600];
        double[]    energy=new double[1600];
        double[]    acfSum=new double[1600];
        int a = 0;
        double pi=3.141592654;
        int index = 0;
        int[] start=new int[10];//儲存有聲音的開始點 
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            filename = "";
        }
        void Done()
        {	
	      
	        int     i, j, bytespersample, samplerate, datasize, samplenumber,
                    totalframes, wsize, frameloc;
            float   fsize_ms, fshift_ms, acfloc=0;

            byte[] b = new byte[44];
            char[]  header1=new char[44];//maybe char
	        char[]   infile=new char[40], outfilename=new char[40];

           
            //printf("\nEnter frame size in ms <eg. 20>:");
            //scanf("%f",&fsize_ms);
            fsize_ms=20;
            //printf("\nEnter frame shift in ms <eg. 10>:");
            //scanf("%f",&fshift_ms);
            fshift_ms=10;

            //in=fopen(infile,"rb");
            fs = new FileStream(@filename, FileMode.Open);
            Console.Write(fs.Length);
            fs.Read(b, index,44);
            for (int tt = 0; tt < 44; tt++)
                header1[tt] = Convert.ToChar(b[tt]);
            index = 44;
   	        numchannels=header1[23]*256+header1[22];
   	        //printf("\nNumber of channals:%d",numchannels);
   	        samplerate=header1[26]*256*256+header1[25]*256+header1[24];
   	       // printf("\nSampling rate:%d",samplerate);
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
            fs.Close();
    
            framesize=(int)(samplerate*fsize_ms/1000);
            frameshift=(int)(samplerate*fshift_ms/1000); 
            totalframes=(samplenumber-framesize)/frameshift;
            //printf("\nTotal number of frames: %d",totalframes);
            //printf("\nEnter output file name:");
            //scanf("%s",outfilename);
            //out=fopen(outfilename,"w");
            //printf("\n Doing short-time energy ...");
            st_energy(totalframes);
           // printf("done.");
            //printf("\n Doing short-time magnitude ...");
            st_magnitude(totalframes);
            //printf("done.");
            //printf("\n Doing short-time zerocrossing ...");
            st_zerocrossing(totalframes);
            //printf("done.");

            //printf("\nEnter ACF location in seconds <eg. 1.76>:");
            //scanf("%f",&acfloc);
            frameloc=(int)(acfloc*1000/fshift_ms+0.5);
            //printf("\nEnter ACF window size L <eg. 400>:");
            //scanf("%d", &wsize);
	        wsize=400;
	
	        int x,framebegin;

	
	
	        //fprintf(out,"\n");
	        //fprintf(out,"\n");
	        for(x=0; x<totalframes; x++){
		        framebegin=x*frameshift;
		        acfSum[x]=acf(framebegin, wsize);
		
		        acfSum[x]=acfSum[x]*logEn[x];
		        if(acfSum[x]!=0.0){
			        logEncount++;
		        }
	
		
		
	        }
	
	
	        //fprintf(out,"\n");
	
	        for(x=0; x<totalframes; x++){
	
	        //	fprintf(out,"%9.5f   ",acfSum[x]);
	        }
	
	
        //	fprintf(out,"\n");
	
	
	
	        double acfTotalSum=0.0;
	        for(x=0; x<totalframes; x++){
	
		        acfTotalSum+=acfSum[x];
	        }
	
        //	fprintf(out,"\n");
        //	fprintf(out,"\n");
	
	
	        double average=0.0;
	        //cout<<acfTotalSum<<"  "<<logEncount<<endl;
	        average=acfTotalSum/logEncount;
	        //cout<<endl<<endl;
	        //cout<<"period: "<<average<<endl;
	        double acfTotalSum2=0.0;
	        double sumCount=0.0;
	        double average2=0.0;
	        for(x=0; x<totalframes; x++){
		        if(acfSum[x]>=(average-40)&&acfSum[x]<=(average+40)){
			        acfTotalSum2+=acfSum[x];
			        sumCount++;
		        }
	        }
	        average2=acfTotalSum2/sumCount;
	        //cout<<"period(filter): "<<average2<<endl;
	        int freq=0;
	        freq=16000/(int)average2;
	        //cout<<"freqency: "<<freq<<endl;
	        if(average2>90.7){
               //pictureBox1.Image=Resource1.Boy;
               pictureBox1.BackgroundImage = Resource1.Boy;
                //textBox1.Text = "Boy";
	        }
	        if (average2<90.7){
                //pictureBox1.Image = Resource1.Girl;
                pictureBox1.BackgroundImage = Resource1.Girl;
	
	        }
            label1.Text = average2.ToString("0.00");
            label2.Text = (16000/average2).ToString("0.00");
            //textBox1.Text = average2.ToString("0.00");
        //	fprintf(out,"\n");
	
	        enprint(totalframes);
            /*printf("\nEnter modified ACF window size L <eg. 200>:");
            scanf("%d", &wsize);
            macf(frameloc, wsize);
            printf("\nEnter AMDF window size L <eg. 400>:");
            scanf("%d", &wsize);
            amdf(frameloc, wsize);
            */
            fs.Close();
        }
        void read_sound_file(int number)
        {
             int      i, bytespersample;
             short    datashort;
             double   sigsum,bias;
              byte[] b=new byte[2];
            bytespersample=bitspersample/8;
            if (bytespersample==2) {                    // regular
                if (numchannels==1) {                   // mono
                    for (i=0; i<number; i++) {
                        //fread(&datashort,2,1,in);
                        fs.Read(b,0,2);
                        datashort=BitConverter.ToInt16(b,0);
                        index+=2;
                        sound[i]=(float)datashort/(float)32768.0;
                    }
                }
                else {                               // stereo to mono
                    for (i=0; i<number; i++) {
                        fs.Read(b, 0, 2);
                        index += 2;
                        datashort=BitConverter.ToInt16(b,0);
                        sound[i]=(float)datashort/(float)32768.0;
                        fs.Read(b, 0,2);
                        index += 2;
                        datashort = BitConverter.ToInt16(b, 0);
                        sound[i]=(sound[i]+(float)datashort/(float)32768.0)/(float)2.0;
                    }
                }
            }
            sigsum=0.0;
            for (i=0; i<number; i++) sigsum=sigsum+sound[i];
            bias=sigsum/(float)number;
            for (i=0; i<number; i++) sound[i]=sound[i]-(float)bias;
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
			         if(logEn[i]<=-10.0){
				         logEn[i]=0.0;
			         }
			        else{ 
				         logEn[i]=1.0;
				 
			         }
             }
    
        }
        void st_magnitude(int totalframes)
        {
             int     i, j, framestart;
             double[]  magnitude=new double[1600];
             for (i=0; i<totalframes; i++) {
                 magnitude[i]=0.0;
                 framestart=i*frameshift;
                 for (j=framestart; j<framestart+framesize; j++) {
                     magnitude[i]=magnitude[i]+Math.Abs(sound[j]);
                 }
             }
            
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
               
              }
        }       

        double acf(int framebegin, int width)
        {
             int      i, j, k,d=0, fstart;
             double   sum, ss1, acf;
	         float[] acf1=new float[405],com=new float[100];

        //     printf("\n %d %d",frameloc, width);
             fstart=framebegin;
             ss1=0.0;
             for (i=fstart; i<fstart+width; i++) ss1=ss1+sound[i]*sound[i];    //做分母 
             for (k=0; k<width; k++) {
                 sum=0.0;
                 for (i=k; i<width; i++) {
                     sum=sum+sound[i+fstart]*sound[i+fstart-k];
                 }
                 acf=sum/ss1;
		         acf1[k]=(float)acf;
                 //fprintf(out,"%9.5f   ",acf);
             }
		        //fprintf(out,"\n");
	         float temp = (float)0.0;
	         int max=0;
	         for(int m=51;m<width;m++)
	         {
		         if(acf1[m+1]<acf1[m] && acf1[m-1]<acf1[m] && acf1[m]>0.4){
			
			        com[d]=acf1[m]; 
			         if (com[d]>temp){
				         temp = com[d];
				         max=m;
			        }

			 
			        d++;
			      }   	 
	         }

	         return max;
        }
        void pitch(int totalframes){
	        int a = 0;
	        int begin,end=0;
	        int[] start=new int[10];//儲存有聲音的開始點 
	        for (int i = 0; i<totalframes; i++){
		        if (energy[i]>0 && zcrarray[i]<0.3){
			        begin = i;
			        //printf("%d\n", begin);
			        for (int j = begin; j<totalframes; j++){
				        if (energy[j] == 0){
					        end = j;
					        //printf("%d\n", end);
					        i = j;
					        break;
				        }

			        }
			        if (end - begin>16){
				        start[a] = begin;
				        //fprintf(out, "\n");
				        //fprintf(out, "%9d   ", start[a]);
				        a++;
			        }

		        }

	        }

        }
        void	enprint(int totalframes){
	
             for(int i=0; i<totalframes; i++){
     	        if(logEn2[i]<=-5.0){				//log(energy) -5.0為老師所要求的閥值 
     		        logEn2[i]=-30.0;
     		        acfSum[i] = 0.0;
		         }
		       
     	 	
	         }
	       
	    
	        int begin, end=0;
	      
	        for (int i = 0; i<totalframes; i++){
		        if (acfSum[i]>0.0 ){					//找有聲音的區間 
			        begin = i;							//當ACF不等於零時 為聲音的起點 
			       
			        for (int j = begin; j<totalframes; j++){
				        if (acfSum[j] == 0.0){			//當ACF回到零時 為終點 
					        end = j;
					       
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
				        a++;
			        }

		        }
		
	 
	        }
	
     
        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {

            if (filename == "")
            {
                MessageBox.Show("未選擇音檔");
            }
            else
            {
                Done();
                index = 0;

                logEncount = 0.0;

                a = 0;
                for (int i = 0; i < 1600; i++)
                {
                    logEn[i] = 0;
                    logEn2[i] = 0;
                    zcrarray[i] = 0;
                    energy[i] = 0;
                    acfSum[i] = 0;
                }
                for (int i = 0; i < 160000; i++)
                    sound[i] = 0;

                for (int i = 0; i < 10; i++)
                    start[i] = 0;

                filename = "";
            }

            pictureBox6.Show();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            clear(); 
            openFileDialog1.Filter = "wav files (*.wav)|*.wav";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
            
                string[] strs = filename.Split('\\');
 
                this.Text = "ISPT-"+strs[strs.Length-1];
            }
            pictureBox6.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Record_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("Praat.exe");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            clear();
            openFileDialog1.Filter = "wav files (*.wav)|*.wav";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = openFileDialog1.FileName;

                string[] strs = filename.Split('\\');

                this.Text = "ISPT-" + strs[strs.Length - 1];
            }
            pictureBox2.Hide();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            vowel v = new vowel();
            label3.Text = v.done(filename);
            pictureBox2.Show();

            this.chart1.Series["Vote"].Points.AddXY("鄧同學", v.pinflag);
            this.chart1.Series["Vote"].Points.AddXY("尤同學", v.iaoflag);
            this.chart1.Series["Vote"].Points.AddXY("莊同學", v.liuflag);
            this.chart1.Series["Vote"].Points.AddXY("范同學", v.fanflag);
        }
        public void clear()
        {
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            pictureBox1.BackgroundImage = null;
        }
    }



}
