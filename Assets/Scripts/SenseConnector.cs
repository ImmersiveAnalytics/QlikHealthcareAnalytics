using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using HoloToolkit.Unity;
using System.Collections.Generic;
using ChartAndGraph;

public class SenseConnector : MonoBehaviour {


    //public GameObject patient;
    public Sankey mySankey;
    public GameObject myBarchart;
    public GameObject cursorForHover;
    public GameObject myPiechart;
    public GameObject myLinechart;
    public Material myChartMaterial;
    public Material[] myPieMaterials;
    public GameObject Canvas;
    public TextToSpeechManager textToSpeechManager;


    // Use this for initialization
    void Awake()
    {
        getPaths();
    }

    void Start () {
        //selectSmokers();
    }

    // Update is called once per frame
    void Update () {
	
	}

    public void getPaths()
    {
        Debug.Log("getting paths");
        textToSpeechManager.StopSpeaking();

        string url = "http://pe.qlik.com:8082/listPaths";
        WWWForm form = new WWWForm();
        form.AddField("field", "val");
        WWW www = new WWW(url, form);
        StartCoroutine(PathRequest(www));
    }

    IEnumerator PathRequest(WWW www)
//    IEnumerator PathRequest(string url)
    {
//        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www;
//        yield return www.Send();

        // check for errors
        if (www.error == null)
//        if (!www.isError)
        {
            string s = www.text;
//            string s = www.downloadHandler.text;
            mySankey.NewData(s);
            getBarchart();
            getPiechart();
            getLinechart();
            getFields();
            getText(s);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    public void getBarchart()
    {
        Debug.Log("getting barchart");

        AddBarData myBarScript = myBarchart.GetComponent<AddBarData>();
        myBarScript.Clear();

        string url = "http://pe.qlik.com:8082/listBars";
        WWWForm form = new WWWForm();
        form.AddField("field", "val");
        WWW www = new WWW(url, form);
        StartCoroutine(BarchartRequest(www));
    }

    IEnumerator BarchartRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;

            AddBarData myBarScript = myBarchart.GetComponent<AddBarData>();
            //myBarScript.Clear();
            

            JSONNode JBars = JSON.Parse(s);
            for (int i = 0; i < JBars.AsArray.Count; i++)
            {
                string b = JBars[i].ToString();
                string bar = b.Substring(1, b.Length - 2);
                string[] kvp = bar.Split(':');
                string key = kvp[0].Substring(1, kvp[0].Length - 2);
                string val = kvp[1].Substring(1, kvp[1].Length - 2);

                myBarScript.AddCategory(key, myChartMaterial);
                myBarScript.SetValue(key, float.Parse(val));
                Debug.Log("bar: " + key + ":" + val);
            }
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    public void barClicked(BarChart.BarEventArgs args)
    {
        selectBar("Admission Type", args.Category);
    }

    public void barHovered(BarChart.BarEventArgs args)
    {
        cursorForHover.SetActive(true);
        Text hoverText = cursorForHover.GetComponentInChildren<Text>();
        hoverText.text = args.Category;
        Debug.Log("bar hovered " + args.Category);
    }

    public void barHoverExit()
    {
        cursorForHover.SetActive(false);
    }

    public void getPiechart()
    {
        Debug.Log("getting piechart");

        string url = "http://pe.qlik.com:8082/listPies";
        WWWForm form = new WWWForm();
        form.AddField("field", "val");
        WWW www = new WWW(url, form);
        StartCoroutine(PiechartRequest(www));
    }

    IEnumerator PiechartRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;

            AddPieData myPieScript = myPiechart.GetComponent<AddPieData>();
            myPieScript.Clear();


            JSONNode JPies = JSON.Parse(s);
            for (int i = 0; i < JPies.AsArray.Count; i++)
            {
                string p = JPies[i].ToString();
                string pie = p.Substring(1, p.Length - 2);
                string[] kvp = pie.Split(':');
                string key = kvp[0].Substring(1, kvp[0].Length - 2);
                string val = kvp[1].Substring(1, kvp[1].Length - 2);

                myPieScript.AddCategory(key, myPieMaterials[i]);
                myPieScript.SetValue(key, float.Parse(val));
                Debug.Log("pie: " + key + ":" + val);
            }
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    public void pieClicked(PieChart.PieEventArgs args)
    {
//        selectPie("AbWVpk", args.Category);
        selectPie("Age Group", args.Category);
    }

    public void getLinechart()
    {
        Debug.Log("getting linechart");

        AddLineData myLineScript = myLinechart.GetComponent<AddLineData>();
        myLineScript.Clear();

        string url = "http://pe.qlik.com:8082/listLines";
        WWWForm form = new WWWForm();
        form.AddField("field", "val");
        WWW www = new WWW(url, form);
        StartCoroutine(LinechartRequest(www));
    }

    IEnumerator LinechartRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;

            AddLineData myLineScript = myLinechart.GetComponent<AddLineData>();
            //myLineScript.Clear();

            JSONNode JLines = JSON.Parse(s);
            for (int i = 0; i < JLines.AsArray.Count; i++)
            {
                string l = JLines[i].ToString();
                string line = l.Substring(1, l.Length - 2);
                string[] kvp = line.Split(':');
                string key = kvp[0].Substring(1, kvp[0].Length - 2);
                string val = kvp[1].Substring(1, kvp[1].Length - 2);

                //myLineScript.AddCategory(key, myPieMaterials[i]);
                //myLineScript.SetValue(float.Parse(key), float.Parse(val));
                myLineScript.SetValue(key, float.Parse(val));
                Debug.Log("line: " + key + ":" + val);
            }
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

//    public void lineClicked(GraphChart.LineEventArgs args)
//    {
//        selectPie("Customer Age", args.Category);
//    }

    public void getText(string data)
    {

        Debug.Log("data: " + data);
        JSONNode JPaths = JSON.Parse(data);
        List<string> _paths = new List<string>();
        for (int i = 0; i < JPaths.AsArray.Count; i++)
        {
            string p = JPaths[i];
            string path = p.Substring(1, p.Length - 2);
            _paths.Add(path);
        }

        string senseData = @"
{
    ""extensionVersion"":""2"",
    ""charts"":[
        {  
            ""inputChart"":{  
                ""type"":""SANKEY_CHART"",
                ""relevantEntryThreshold"":-1,
                ""relevantExitThreshold"":-1,
                ""nbrPathThreshold"":-1,
                ""multiSeriesType"":""UNRELATED""
            },
            ""outputText"":{  
                ""levelOfDetail"":7,
                ""persona"":""MANAGER"",
                ""lang"":""en""
            },
            ""dimensions"":[
                {  
                    ""label"":""Surgical Path"",
                    ""technicalLabel"":false,
";
        senseData += "\"cardinal\":" + _paths.Count.ToString() + ",";

        senseData += @"
                    ""othersLabel"":""Others"",
                    ""tags"":[
                        ""$ascii"",
                        ""$text""
                    ]
                }
            ],
            ""measures"":[
                {  
                    ""label"":""patients"",
                    ""technicalLabel"":false,
                    ""defaultValue"":""0"",
                    ""meaningOfUp"":""GOOD"",
                    ""unit"":""""
                }
            ],
            ""facts"":[
";

        int c = 1;
        foreach (string p in _paths)
        {
            //Debug.Log("_path: " + p);
            string[] sVals = p.Split(',');
            List<string> vals = new List<string>();
            foreach(string s in sVals)
            {
                vals.Add(s);
            }
            // Fix for paths with only a single node
            if (vals.Count == 1)
            {
                vals.Add(" " + vals[0]);
            }
            string newPath = string.Join(",", vals.ToArray());
            senseData += @"{ ""dimensions"":[ { ""index"":0, ""position"":" + c.ToString() + @", ""label"":""" + newPath + @"""} ],";
            senseData += @"""measures"":[ { ""index"":0, ""value"":""1"" } ] }";
            if (c < _paths.Count)
                senseData += ",";
            c++;
        }

        senseData = senseData + @"
            ]
        }
    ]
}";
        Debug.Log("senseData: " + senseData);

        // Add custom headers to the request.
        Dictionary<string, string> headers = new Dictionary<string, string>();
//        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(
//            System.Text.Encoding.ASCII.GetBytes("eyJraWQiOiJzdGFnaW5nXzAiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJTYXZ2eSIsImF1ZCI6InN0YWdpbmciLCJleHAiOjE1MDMxNDcxMzEsImp0aSI6IjRQVXYtbHl1OTdvaTlkMGUxNTFOOUEiLCJpYXQiOjE0NzE2MTExMzEsInN1YiI6Ik1hcmsgQmx1bmRyZWQiLCJ0b29scyI6WyJxbGlrc2Vuc2UiXSwiZW1haWwiOiJtYmx1bmRyZWRAeXNlb3AuY29tIn0.2zFzGT5rYDTwprW7WPV5nJtYnPyfMfRNPNx05bh5MQE23Ju0mdiTcpls7UR845-8y1eOb0WcjRwBsONEn1kWbg:"));
        headers["Authorization"] = "Basic " + System.Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes("eyJraWQiOiJwcm9kXzAiLCJhbGciOiJFUzI1NiJ9.eyJpc3MiOiJTYXZ2eSIsImF1ZCI6InByb2QiLCJleHAiOjE1MTg2NjEzNDcsImp0aSI6ImVBTkdrY29WX2tFSlF3bE1NemhONFEiLCJpYXQiOjE0ODcxMjUzNDcsInN1YiI6Ik1hcmdvbGlzIFRvZGQiLCJ0b29scyI6WyJxbGlrc2Vuc2UiXSwiZW1haWwiOiJ0b2RkLm1hcmdvbGlzQHFsaWsuY29tIn0.XcjNksZuAYhmdDlzzoDTBM6zWO6EXfnNFzdUnEDgU1hEIKFtEnUZCSbjWJgdhBzMZjZwIcboJYRvJwR6Mb5vdw:"));
        headers["Content-Type"] = "application/json;charset=UTF-8";
        headers["Accept"] = "text/html";
        headers["Content-Length"] = senseData.Length.ToString();
        Debug.Log("Yseop Request header: " + headers["Authorization"]);

        // Post a request to an URL with our custom headers
        //string url = "https://savvy-qlik-staging.yseop-cloud.com/yseop-manager/direct/savvy-kb-fr/dialog.do";
        string url = "https://savvy-api.yseop-cloud.com/sandbox/api/v1/describe-chart";
    
        var encoding = new System.Text.UTF8Encoding();
        WWW www = new WWW(url, encoding.GetBytes(senseData), headers);
        StartCoroutine(TextRequest(www));
    }


    IEnumerator TextRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;
            Debug.Log("Yseop response: " + s);

            int start = s.IndexOf("text-result") + 13;
            int end = s.IndexOf("<p>");
            string text = s.Substring(start, end-start);
            text = text.Replace("\n", string.Empty);
            text = text.Replace("\r", string.Empty);
            //text = text.Replace("\t", String.Empty);

            Debug.Log("Yseop text: " + text);

            //            textToSpeechManager.SpeakText("Your flow of 14 Patients starts from only one entry, which is ALVEOLOPLASTY. Overall, 28.57 % of Patients end at the main exit which is SURG TOOTH EXTRACT NEC (4 Patients). Then, there are CONT MECH VENT < 96 HRS with 2, and finally TOOTH EXTRACTION NEC with 2.");
            textToSpeechManager.SpeakText(text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    public void getFields()
    {
        Debug.Log("getting fields");

        string url = "http://pe.qlik.com:8082/getFieldStates";
        WWWForm form = new WWWForm();
        form.AddField("field", "val");
        WWW www = new WWW(url,form);
        StartCoroutine(FieldRequest(www));
    }

    IEnumerator FieldRequest(WWW www)
    {
        //        UnityWebRequest wwwF = UnityWebRequest.Get(url);
//        yield return wwwF.Send();
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;
            mySankey.updateUI(s);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    public void clear()
    {
        Debug.Log("clearing selections");
        string url = "http://pe.qlik.com:8082/clear";
        WWWForm form = new WWWForm();
        form.AddField("field", "val");
        WWW www = new WWW(url, form);

        StartCoroutine(ClearSelections(www));
    }
    public void selectResponsiveVoice()
    {
        if (Canvas.transform.Find("Toggle-Responsive-Y").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-Responsive-Y").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectResponsiveNoVoice()
    {
        if (Canvas.transform.Find("Toggle-Responsive-N").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-Responsive-N").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectResponsiveY()
    {
        if (Canvas.transform.Find("Toggle-Responsive-Y").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Responsive - Y");
            StartCoroutine(FilterSelect("Responsive", "Y"));
        }
    }
    public void selectResponsiveN()
    {
        if (Canvas.transform.Find("Toggle-Responsive-N").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Responsive - N");
            StartCoroutine(FilterSelect("Responsive", "N"));
        }
    }
    public void selectBleedingVoice()
    {
        if (Canvas.transform.Find("Toggle-Bleeding-Y").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-Bleeding-Y").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectBleedingNpVoice()
    {
        if (Canvas.transform.Find("Toggle-Bleeding-N").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-Bleeding-N").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectBleedingY()
    {
        if (Canvas.transform.Find("Toggle-Bleeding-Y").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Bleeding - Y");
            StartCoroutine(FilterSelect("Bleeding", "Y"));
        }
    }
    public void selectBleedingN()
    {
        if (Canvas.transform.Find("Toggle-Bleeding-N").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Bleeding - N");
            StartCoroutine(FilterSelect("Bleeding", "N"));
        }
    }
    public void selectBMIVoice()
    {
        if (Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectBMIVoiceNo()
    {
        if (Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectBMIY()
    {
        if (Canvas.transform.Find("Toggle-BMI-Y").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Overweight - Y");
            StartCoroutine(FilterSelect("Overweight", "Y"));
        }
    }
    public void selectBMIN()
    {
        if (Canvas.transform.Find("Toggle-BMI-N").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Overweight - N");
            StartCoroutine(FilterSelect("Overweight", "N"));
        }
    }
    public void selectMaleVoice()
    {
        if (Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectMale()
    {
        if (Canvas.transform.Find("Toggle-Male").GetComponent<Toggle>().isOn)
        {
            Debug.Log("selecting Males");
            StartCoroutine(FilterSelect("Gender", "Male"));
        }
    }
    public void selectFemaleVoice()
    {
        if (Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().interactable)
        {
            Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().isOn = true;
        }
    }
    public void selectFemale()
    {
        if(Canvas.transform.Find("Toggle-Female").GetComponent<Toggle>().isOn)
        { 
            Debug.Log("selecting Females");
            StartCoroutine(FilterSelect("Gender", "Female"));
        }
    }

    public void selectBar(string cat, string val)
    {
        Debug.Log("selecting " + val + " from " + cat + " BarChart");
        StartCoroutine(FilterSelect(cat, val));
    }
    public void selectPie(string cat, string val)
    {
        Debug.Log("selecting " + val + " from " + cat + " PieChart");
        StartCoroutine(FilterSelect(cat, val));
    }


    IEnumerator FilterSelect(string field, string val)
    {
        WWWForm form = new WWWForm();
        form.AddField("fieldName", field);
        form.AddField("fieldValue", val);

//        UnityWebRequest www = UnityWebRequest.Post("http://pe.qlik.com:8082/filter", form);
        WWW www = new WWW("http://pe.qlik.com:8082/filter", form);
//        yield return www.Send();
        yield return www;

//        if (www.isError)
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log(www.error);
        }
        else
        {
//            string s = www.downloadHandler.text;
            string s = www.text;
            s = s.Replace(',', '\n');
            Debug.Log(s);
            Debug.Log("selection POST complete!");

            yield return new WaitForSeconds(1);
            getPaths();
        }
    }

    IEnumerator ClearSelections(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            string s = www.text;
            Debug.Log(s);
            Debug.Log("selection clear complete!");

            yield return new WaitForSeconds(1);
            mySankey.clearUI();
            getPaths();
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }
}
