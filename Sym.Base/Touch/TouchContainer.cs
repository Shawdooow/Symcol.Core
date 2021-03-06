﻿#region usings

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osuTK;
using osuTK.Graphics;

#endregion

namespace Sym.Base.Touch
{
    /// <summary>
    /// This class is a hack, don't take it seriously (but it should work on ios so...)
    /// </summary>
    public class TouchContainer : Container
    {
        //public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected readonly Box Box;

        public readonly SpriteText SpriteText;

        public Action OnTap;

        public Action OnRelease;

        public bool Tapped { get; set; }

        protected bool Hovered { get; set; }

        protected static bool Pressed { get; set; }

        public TouchContainer()
        {
            Clock = new DecoupleableInterpolatingFramedClock();

            Size = new Vector2(100);
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                Box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = Color4.OrangeRed,
                },
                (SpriteText = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = "Venera",
                    TextSize = 16,
                    Alpha = 0.75f,
                    Text = "Text",
                }).WithEffect(new GlowEffect
                {
                    Colour = Color4.Transparent,
                    PadExtent = true,
                }),
            };

            Masking = true;
            BorderColour = Color4.White;
            BorderThickness = 6;
            EdgeEffect = new EdgeEffectParameters
            {
                Hollow = true,
                Colour = Color4.Cyan,
                Type = EdgeEffectType.Shadow,
                Radius = 8,
            };
        }

        public virtual void Tap()
        {
            Tapped = true;
            OnTap?.Invoke();

            Box.FadeTo(0.25f, 200);
            BorderColour = Color4.OrangeRed;
        }

        public virtual void Release()
        {
            Tapped = false;
            OnRelease?.Invoke();

            Box.FadeOut(200);
            BorderColour = Color4.White;
        }

        protected override bool OnHover(HoverEvent e)
        {
            Hovered = true;
            if (Pressed && !Tapped)
                Tap();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            Hovered = false;
            if (Tapped)
                Release();
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Pressed = true;
            if (Hovered && !Tapped)
                Tap();
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            Pressed = false;
            if (Hovered && Tapped)
                Release();
            return base.OnMouseUp(e);
        }
    }
}
