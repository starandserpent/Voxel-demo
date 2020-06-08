using System;
using Godot;
public class PlayerCamera : Camera {
    //var currVal : String# Shader params
    [Export] public float horizontalFOV = 140.0f;
    [Export] public float strength = 0.5f;
    [Export] public float cylindricalRatio = 2f;
    private float height = 0;
    private Vector2 screenSize = new Vector2 (0, 0);
    private float aspect = 0.0f;

    private Button checkButton;
    private Slider FOVslider;

    private LineEdit barrelPower;

    private Label label;

    private Label label2;

    private Slider cylinratio;

    private TextureRect rect;

    public override void _EnterTree () {
        checkButton = (Button) FindNode ("CheckButton");
        FOVslider = (Slider) FindNode ("FOVSlider");
        barrelPower = (LineEdit) FindNode ("BarrelPower");
        label = (Label) FindNode ("ShaderPropLabel2");
        label2 = (Label) FindNode ("ShaderPropLabel4");
        cylinratio = (Slider) FindNode ("cylinratio");
        rect = (TextureRect) FindNode ("BarrelDist");

        barrelPower.Connect ("text_entered", this, nameof (OnBarrelPowerTextChanged));
        FOVslider.Connect ("value_changed", this, nameof (OnFOVSliderValueChanged));
        checkButton.Connect ("pressed", this, nameof (OnCheckButtonPressed));
        cylinratio.Connect ("value_changed", this, nameof (OnCylinratioValueChanged));

        float fov = (float) (Math.Atan (height) * 2 * 180 / Math.PI);
        FOVslider.Value = fov;
        barrelPower.Text = strength.ToString ();
        label.Text = "FOV: " + horizontalFOV;

        checkButton.Pressed = false;
        horizontalFOV = fov;
        screenSize.x = GetViewport ().GetVisibleRect ().Size.x; //Get Width
        screenSize.y = GetViewport ().GetVisibleRect ().Size.y; //Get Height
        aspect = screenSize.x / screenSize.y;

        height = (float) ((Math.Tan ((Math.PI / 180) * horizontalFOV) / 2.0) / aspect);

        cylinratio.Value = cylindricalRatio;

        ((ShaderMaterial) rect.Material).SetShaderParam ("strength ", strength);
        ((ShaderMaterial) rect.Material).SetShaderParam ("height ", height);
        ((ShaderMaterial) rect.Material).SetShaderParam ("spectRatio ", aspect);
        ((ShaderMaterial) rect.Material).SetShaderParam ("cylindricalRatio", cylindricalRatio);
    }

    //uniform float BarrelPower =1.1;
    public void OnBarrelPowerTextChanged (string newText) {
        if (newText.IsValidFloat ()) {
            float currVal = newText.ToFloat ();
            ((ShaderMaterial) rect.Material).SetShaderParam ("strength", currVal);
        }
        barrelPower.Text = newText;
    }

    public void OnFOVSliderValueChanged (float value) {
        horizontalFOV = value;
        height = (float) (Math.Tan (((Math.PI / 180) * horizontalFOV) / 2.0) / aspect);

        float fov = (float) (Math.Atan (height) * 2 * 180 / Math.PI);
        label.Text = "FOV: " + horizontalFOV.ToString ();
        ((ShaderMaterial) rect.Material).SetShaderParam ("height", height);
    }

    public void OnCheckButtonPressed () {
        if (checkButton.Pressed) {
            rect.Show ();
        } else {
            rect.Hide ();
        }
    }

    public void OnCylinratioValueChanged (float value) {
        cylindricalRatio = value;
        label2.Text = "Cyln Ratio: " + cylindricalRatio.ToString ();
        ((ShaderMaterial) rect.Material).SetShaderParam ("cylindricalRatio", cylindricalRatio);
    }
}