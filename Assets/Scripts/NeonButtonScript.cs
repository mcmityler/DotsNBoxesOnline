using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NeonButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler// required interface when using the OnPointerEnter method.
{

    [SerializeField] private GameObject _backgroundBlurImg;
    [SerializeField] private GameObject _ButtonText;
    private GameObject _TitleText;

    private Color32 _myColour = new Color32(0, 75, 0, 255);
    private Color32 _invertedColour;

    private Color32 _blurSQRColor;

    public bool isPauseMenu = false;

    void Start()
    {
        _TitleText = GameObject.FindGameObjectWithTag("MainTitle");
        InitColour();
        InvertColour();

    }
    void Update()
    {
        if (isPauseMenu)
        {
            _ButtonText.GetComponent<TMP_Text>().UpdateMeshPadding();
            if (Input.GetButtonDown("Escape"))
            {
                InitColour();
            }
        }
    }
    public void InitColour()
    {

        if (_ButtonText != null)
        {
            _backgroundBlurImg.GetComponent<Image>().color = _myColour;
            _ButtonText.GetComponent<TMP_Text>().fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, _myColour);// Instead of using a string to access FFthe material property, you could use the ShaderUtilities class I provide
                                                                                                                      // Since some of the material properties can affect the mesh (size) you would need to update the padding values.
            _ButtonText.GetComponent<TMP_Text>().UpdateMeshPadding();
        }
        if (_TitleText != null)
        {
            _TitleText.GetComponent<TMP_Text>().fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, _myColour);// Instead of using a string to access FFthe material property, you could use the ShaderUtilities class I provide
            // Since some of the material properties can affect the mesh (size) you would need to update the padding values.
            _TitleText.GetComponent<TMP_Text>().UpdateMeshPadding();
        }
        if (_ButtonText == null)
        {
            BlurSQRColour();
            _backgroundBlurImg.GetComponent<Image>().color = _blurSQRColor;
        }

    }
    public void ChangeColour(Color32 m_newColour)
    {
        _myColour = m_newColour;
        InitColour();
        InvertColour();
    }
    public void InvertColour()
    {
        int m_red = (255 - (int)_myColour[0]) / 3;
        int m_green = (255 - (int)_myColour[1]) / 3;
        int m_blue = (255 - (int)_myColour[2]) / 3;
        //Debug.Log(m_red + " " + m_blue+ " " + m_green);
        _invertedColour = new Color32((byte)m_red, (byte)m_green, (byte)m_blue, 255);
    }
    public void BlurSQRColour()
    { //what colour my blur squares are (just makes it my normal colour but lowers its opacity)
        _blurSQRColor = new Color32((byte)_myColour[0], (byte)_myColour[1], (byte)_myColour[2], 150);
    }
    //Do this when the cursor enters the rect area of this selectable UI object.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_ButtonText != null)
        {
            Debug.Log("The cursor entered the selectable UI element.");
            _backgroundBlurImg.GetComponent<Image>().color = _invertedColour;

            _ButtonText.GetComponent<TMP_Text>().fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, _invertedColour);// Instead of using a string to access FFthe material property, you could use the ShaderUtilities class I provide
                                                                                                                            // Since some of the material properties can affect the mesh (size) you would need to update the padding values.
            _ButtonText.GetComponent<TMP_Text>().UpdateMeshPadding();
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_ButtonText != null)
        {
            Debug.Log("The cursor Left the selectable UI element.");
            _backgroundBlurImg.GetComponent<Image>().color = _myColour;
            _ButtonText.GetComponent<TMP_Text>().fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, _myColour);// Instead of using a string to access FFthe material property, you could use the ShaderUtilities class I provide
                                                                                                                      // Since some of the material properties can affect the mesh (size) you would need to update the padding values.
            _ButtonText.GetComponent<TMP_Text>().UpdateMeshPadding();
        }
    }
    public void OnButtonClick()
    {
        InitColour();
    }
}
