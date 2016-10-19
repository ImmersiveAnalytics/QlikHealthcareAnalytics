using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA.Input;
using UnityEngine.Windows.Speech;
using HoloToolkit.Unity;
using System;

public class TextToSpeechManagerTest : MonoBehaviour
{
    private GestureRecognizer gestureRecognizer;
    public TextToSpeechManager textToSpeechManager;

    // Use this for initialization
    void Start ()
    {
        // Set up a GestureRecognizer to detect Select gestures.
        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent;
        gestureRecognizer.StartCapturingGestures();

    }

    private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        GazeManager gm = GazeManager.Instance;
        if (gm.Hit)
        {
            // Get the target object
            GameObject obj = gm.HitInfo.collider.gameObject;

            // Try and get a TTS Manager
            TextToSpeechManager tts = null;
            if (obj != null)
            {
                tts = obj.GetComponent<TextToSpeechManager>();
            }

            // If we have a text to speech manager on the target object, say something.
            // This voice will appear to emanate from the object.
            if (tts != null)
            {
//                tts.SpeakText("This voice should sound like it's coming from the object you clicked. Feel free to walk around and listen from different angles.");
//                tts.SpeakText("The time is " + DateTime.Now.ToString("t"));
                tts.SpeakText("Your flow of 14 Patients starts from only one entry, which is ALVEOLOPLASTY. Overall, 28.57 % of Patients end at the main exit which is SURG TOOTH EXTRACT NEC (4 Patients). Then, there are CONT MECH VENT < 96 HRS with 2, and finally TOOTH EXTRACTION NEC with 2. ALVEOLOPLASTY: The Patients who began at ALVEOLOPLASTY and finished at SURG TOOTH EXTRACT NEC took only one relevant path: 2 Patients began at ALVEOLOPLASTY before ending at SURG TOOTH EXTRACT NEC. Out of the 4 Patients who began at ALVEOLOPLASTY and resulted at SURG TOOTH EXTRACT NEC, 25 % passed at OTHER GASTROSTOMY, REMOV GASTROSTOMY TUBE, OTHER ORTHODONTIC OPERAT. The Patients who began at ALVEOLOPLASTY and finished at PROSTHET DENTAL IMPLANT took only one relevant path: 1 Patients began at ALVEOLOPLASTY before ending, by a five times repetition, at PROSTHET DENTAL IMPLANT. Out of the 2 Patients who began at ALVEOLOPLASTY and resulted at PROSTHET DENTAL IMPLANT, 50 % passed at SURG TOOTH EXTRACT NEC. The Patients who began at ALVEOLOPLASTY and finished at CONT MECH VENT < 96 HRS took only one relevant path: 1 Patients began at ALVEOLOPLASTY, then to TOOTH EXTRACTION NEC and finally ending at CONT MECH VENT < 96 HRS. Out of the 2 Patients who began at ALVEOLOPLASTY and resulted at CONT MECH VENT < 96 HRS, 100 % passed at TOOTH EXTRACTION NEC, SPINAL TAP, AORTA - SUBCLV - CAROT BYPAS, HEAD / NECK VES RESEC-REPL, REP VESS W SYNTH PATCH, VENOUS CATH NEC, EXTRACORPOREAL CIRCULAT, CARDIOPULM RESUSCITA NOS, ARTERIAL CATHETERIZATION, INSERT ENDOTRACHEAL TUBE. The Patients who began at ALVEOLOPLASTY and finished at TOOTH EXTRACTION NEC took only one path: 2 Patients began at ALVEOLOPLASTY before ending at TOOTH EXTRACTION NEC. ALVEOLOPLASTY is the step that should be focused on to reduce the number of Patients who reach SURG TOOTH EXTRACT NEC");
                }
        }
    }

    public void SpeakTime()
    {
        // Say something using the text to speech manager on THIS test class (the "global" one).
        // This voice will appear to follow the user.
        textToSpeechManager.SpeakText("The time is " + DateTime.Now.ToString("t"));
    }
}
