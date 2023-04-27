using System.Net.Http.Headers;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;

namespace NP2PClient
{
    public partial class frmMain : Form
    {
        string serverAddr = "http://119.91.202.207:10081";
        bool runNP2P = true;
        bool applyOpenPose = true;
        bool applyHed = true;
        public frmMain()
        {
            InitializeComponent();

            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(1000);
                GetReadme();
                Thread.Sleep(1000);

                while (runNP2P && this.Visible)
                {
                    CheckTask();

                    Thread.Sleep(100);
                }
            })).Start();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            runNP2P = false;
        }

        private void GetReadme()
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var readmeStr = webClient.DownloadString(serverAddr+"/np2p/readme.txt");

                    this.Invoke(new Action(() => { tbReadme.Text = readmeStr; }));
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CheckTask()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), serverAddr + "/api/NP2P/NewFrame"))
                    {
                        request.Headers.TryAddWithoutValidation("accept", "application/json");
                        request.Content = new StringContent("");
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var ret = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

                        JObject obj = JObject.Parse(ret);

                        if (obj["status"].ToString().ToLower() == "true")
                        {
                            var frame = JsonConvert.DeserializeObject<ThreejsPoseFrame>(obj["data"].ToString());

                            RunSDApi(frame);
                        }
                    }
                }
            }
            catch { }
        }

        private void RunSDApi(ThreejsPoseFrame frame)
        {
            if (frame.Seed == "") frame.Seed = "-1";

            byte[] originImgBytes = Convert.FromBase64String(frame.FrameImage.Replace("data:image/png;base64,", ""));
            Image originImg = Image.FromStream(new MemoryStream(originImgBytes));

            using (var httpClientFrame = new HttpClient())
            {
                using (var requestFrame = new HttpRequestMessage(new HttpMethod("POST"), "http://localhost:7860/sdapi/v1/txt2img"))
                {
                    requestFrame.Headers.TryAddWithoutValidation("accept", "application/json");

                    JObject objReq = JObject.Parse(@"
{
  ""enable_hr"": false,
  ""denoising_strength"": 0,
  ""firstphase_width"": 0,
  ""firstphase_height"": 0,
  ""hr_scale"": 2,
  ""hr_upscaler"": """",
  ""hr_second_pass_steps"": 0,
  ""hr_resize_x"": 0,
  ""hr_resize_y"": 0,
  ""prompt"": """ + frame.Prompt + @""",
  ""styles"": [
    """"
  ],
  ""seed"": " + frame.Seed + @",
  ""subseed"": -1,
  ""subseed_strength"": 0,
  ""seed_resize_from_h"": -1,
  ""seed_resize_from_w"": -1,
  ""sampler_name"": ""Euler a"",
  ""batch_size"": 1,
  ""n_iter"": 1,
  ""steps"": 50,
  ""cfg_scale"": 7,
  ""width"": " + originImg.Width + @",
  ""height"": " + originImg.Height + @",
  ""restore_faces"": false,
  ""tiling"": false,
  ""do_not_save_samples"": false,
  ""do_not_save_grid"": false,
  ""negative_prompt"": ""easynegative"",
  ""eta"": 0,
  ""s_churn"": 0,
  ""s_tmax"": 0,
  ""s_tmin"": 0,
  ""s_noise"": 1,
  ""override_settings"": {},
  ""override_settings_restore_afterwards"": true,
  ""script_args"": [],
  ""sampler_index"": ""Euler"",
  ""script_name"": """",
  ""send_images"": true,
  ""save_images"": true,
  ""alwayson_scripts"": {
    ""ControlNet"":{ ""args"": []}
  }
}
"
);
                    JArray arrControlNet = objReq["alwayson_scripts"]["ControlNet"]["args"] as JArray;

                    if (applyOpenPose)
                    {
                        arrControlNet.Add(JObject.Parse(@"
{
    'enabled': true,
    'module': 'openpose_full',
    'input_image': '" + frame.FrameImage + @"',
    'mask': null,
    'model': 'control_v11p_sd15_openpose [cab727d4]',
    'weight': 1,
    'invert_image': false,
    'resize_mode': 'Inner Fit (Scale to Fit)',
    'rgbbgr_mode': false,
    'lowvram': false,
    'processor_res': 512,
    'threshold_a': 64,
    'threshold_b': 64,
    'guidance_start': 0,
    'guidance_end': 1,
    'guessmode': false
}"));
                    }

                    if (applyHed)
                    {
                        arrControlNet.Add(JObject.Parse(@"
{
    'enabled': true,
    'module': 'scribble_hed',
    'input_image': '" + frame.FrameImage + @"',
    'mask': null,
    'model': 'control_v11p_sd15_scribble [d4ba51ff]',
    'weight': 1,
    'invert_image': false,
    'resize_mode': 'Inner Fit (Scale to Fit)',
    'rgbbgr_mode': false,
    'lowvram': false,
    'processor_res': 512,
    'threshold_a': 64,
    'threshold_b': 64,
    'guidance_start': 0,
    'guidance_end': 1,
    'guessmode': false
}"));
                    }

                    requestFrame.Content = new StringContent(objReq.ToString());
                    requestFrame.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var retFrame = httpClientFrame.SendAsync(requestFrame).Result.Content.ReadAsStringAsync().Result;

                    JObject outFrame = JObject.Parse(retFrame);

                    frame.ResultImage = outFrame["images"][0].ToString();

                    SaveResultFrame(frame);
                    //Convert.FromBase64String(outFrame["images"][0].ToString())
                }
            }
        }

        private void SaveResultFrame(ThreejsPoseFrame frame)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), serverAddr + "/api/NP2P/UploadImage64"))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");
                    JObject objRequest = new JObject();
                    objRequest["frameId"] = frame.FrameId;
                    objRequest["resultImage"] = frame.ResultImage;
                    request.Content = new StringContent(objRequest.ToString());
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var ret = httpClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }


    public class ThreejsPoseFrame
    {
        public int FrameId { get; set; }
        public Guid? ThreejsPoseTaskId { get; set; }
        public string FrameImage { get; set; }
        public string FrameStatus { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? GenerateDate { get; set; }
        public int? FrameIndex { get; set; }
        public string ResultImage { get; set; }
        public string Prompt { get; set; }
        public string Seed { get; set; }
        public DateTime? PaintDate { get; set; }
    }

    public class ThreejsPoseTask
    {
        public Guid ThreejsPoseTaskId { get; set; }
        public string TaskStatus { get; set; }
        public string VideoPath { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? GenerateDate { get; set; }
    }
}