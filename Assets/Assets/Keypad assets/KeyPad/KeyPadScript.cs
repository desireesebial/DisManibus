using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyPadScript : MonoBehaviour
{
    public int[] Code;
    public string CodeLength;
    private int Presses;
    private string result;
    
    private string ScreenText;
    public GameObject Screen;
    public string Correct;
    private int reset;

    void Start()
    {
      
        Code = new int[(Convert.ToInt32(CodeLength))];
        Presses = 0;
    }
    void Update()
    {
        ScreenText = string.Join("", Code.Select(i => i.ToString()).ToArray());
        Screen.GetComponent<TMPro.TextMeshPro>().text = ScreenText;

        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10))
            {
                
               if(Presses < Convert.ToInt32(CodeLength))
                {
                    if (hit.transform.gameObject.name == "Base") { }
                    else
                    {
                        Debug.Log(hit.transform.gameObject.name);
                        Code[Presses] = hit.transform.gameObject.GetComponent<Number>().number;
                        Presses += 1;
                    }
                }
               if (Presses == Convert.ToInt32(CodeLength))
                {
                   result = String.Join("", new List<int>(Code).ConvertAll(i => i.ToString()).ToArray());
                    Debug.Log(result);
                    if(Correct == result)
                    {
                        // This is where you can add you're own code that will run when the correct code has been entered
                        Debug.Log("The Code Entered Is Correct");
                    }
                    else
                    {
                        Presses = 0;
                        reset = Convert.ToInt32(CodeLength)-1;
                        do
                        {
                            Code[reset] = 0;
                            reset -= 1;
                        } while (reset > -1);

                    }
                }
            }

        }
    }
}
