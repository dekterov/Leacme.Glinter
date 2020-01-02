// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;

public class Hud : Node2D {

	private Color flashColor = Color.FromHsv(1, 1, 1);
	private float rate = 10;
	private float time = 0.0f;
	private ColorPickerButton colorBt = new ColorPickerButton();

	private TextureRect vignette = new TextureRect() {
		Expand = true,
		Texture = new GradientTexture() {
			Gradient = new Gradient() { Colors = new[] { Colors.Transparent } }
		},
		Material = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type canvas_item;
					void fragment() {
						float iRad = 0.3;
						float oRad = 1.0;
						float opac = 0.5;
						vec2 uv = SCREEN_UV;
					    vec2 cent = uv - vec2(0.5);
					    vec4 tex = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
					    vec4 col = vec4(1.0);
					    col.rgb *= 1.0 - smoothstep(iRad, oRad, length(cent));
					    col *= tex;
					    col = mix(tex, col, opac);
					    COLOR = col;
					}"
			}
		}
	};

	public override void _Ready() {
		InitVignette();

		var btHolder = new HBoxContainer();
		btHolder.MarginTop = btHolder.MarginLeft = 20;
		btHolder.AddConstantOverride("separation", 20);
		AddChild(btHolder);

		colorBt.Connect("color_changed", this, nameof(OnNewColorPicked));
		colorBt.Color = flashColor;
		colorBt.RectMinSize = new Vector2(100, 20);
		btHolder.AddChild(colorBt);

		var rateSlider = new HSlider();
		rateSlider.RectMinSize = new Vector2(200, 20);
		rateSlider.MaxValue = 70;
		rateSlider.Value = rate;
		rateSlider.Connect("value_changed", this, nameof(OnNewRatePicked));
		btHolder.AddChild(rateSlider);

	}

	public void OnNewColorPicked(Color color) {
		flashColor = color;
	}

	private void OnNewRatePicked(float val) {
		rate = val;
	}

	public override void _Draw() {
		DrawRect(GetViewportRect(), flashColor);
		DrawBorder(this);
	}

	private void InitVignette() {
		vignette.RectMinSize = GetViewportRect().Size;
		AddChild(vignette);
		if (Lib.Node.VignetteEnabled) {
			vignette.Show();
		} else {
			vignette.Hide();
		}
	}

	public override void _Process(float delta) {
		time += delta;
		var amp = 0.5f;
		flashColor.ToHsv(out var hue, out var sat, out var val);
		flashColor = (rate.Equals(0)) ? colorBt.Color : Color.FromHsv(hue, sat, (float)(amp * Math.Sin(time * rate)) + amp);
		Update();
	}

	public static void DrawBorder(CanvasItem canvas) {
		if (Lib.Node.BoderEnabled) {
			var vps = canvas.GetViewportRect().Size;
			int thickness = 4;
			var color = new Color(Lib.Node.BorderColorHtmlCode);
			canvas.DrawLine(new Vector2(0, 1), new Vector2(vps.x, 1), color, thickness);
			canvas.DrawLine(new Vector2(1, 0), new Vector2(1, vps.y), color, thickness);
			canvas.DrawLine(new Vector2(vps.x - 1, vps.y), new Vector2(vps.x - 1, 1), color, thickness);
			canvas.DrawLine(new Vector2(vps.x, vps.y - 1), new Vector2(1, vps.y - 1), color, thickness);
		}
	}
}
