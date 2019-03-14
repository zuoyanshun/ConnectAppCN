using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.service;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.components {
    public class InputField : StatefulWidget {
        public InputField(
            Key key = null,
            TextEditingController controller = null,
            FocusNode focusNode = null,
            bool obscureText = false,
            bool autocorrect = true,
            TextStyle style = null,
            TextAlign textAlign = TextAlign.left,
            int maxLines = 1,
            bool autofocus = false,
            string hintText = null,
            TextStyle hintStyle = null,
            string labelText = null,
            TextStyle labelStyle = null,
            Color cursorColor = null,
            TextInputAction textInputAction = TextInputAction.none,
            float height = 44.0f,
            ValueChanged<string> onChanged = null,
            ValueChanged<string> onSubmitted = null,
            EdgeInsets scrollPadding = null
        ) : base(key) {
            this.controller = controller ?? new TextEditingController("");
            this.textAlign = textAlign;
            this.focusNode = focusNode;
            this.obscureText = obscureText;
            this.autocorrect = autocorrect;
            this.style = style;
            this.textAlign = textAlign;
            this.maxLines = maxLines;
            this.autofocus = autofocus;
            this.hintText = hintText;
            this.hintStyle = hintStyle;
            this.labelText = labelText;
            this.labelStyle = labelStyle;
            this.height = height;
            this.cursorColor = cursorColor;
            this.textInputAction = textInputAction;
            this.onChanged = onChanged;
            this.onSubmitted = onSubmitted;
            this.scrollPadding = scrollPadding;
        }

        public readonly TextEditingController controller;
        public readonly FocusNode focusNode;
        public readonly bool obscureText;
        public readonly bool autocorrect;
        public readonly TextStyle style;
        public readonly TextAlign textAlign;
        public readonly int maxLines;
        public readonly bool autofocus;
        public readonly string hintText;
        public readonly TextStyle hintStyle;
        public readonly string labelText;
        public readonly TextStyle labelStyle;
        public readonly Color cursorColor;
        public readonly TextInputAction textInputAction;
        public readonly float height;
        public readonly ValueChanged<string> onChanged;
        public readonly ValueChanged<string> onSubmitted;

        public readonly EdgeInsets scrollPadding;
//        public readonly TextInputAction textInputAction;
//        public readonly TextInputType keyboardType;
//        public readonly Brightness keyboardAppearance;

        public override State createState() {
            return new _InputField();
        }
    }

    internal class _InputField : State<InputField> {
        private FocusNode _focusNode;
        private bool _isHintTextHidden = false;

        public override void initState() {
            base.initState();
            _focusNode = new FocusNode();
            widget.controller.addListener(_controllerListener);
        }
        
        public override void dispose() {
            widget.controller.removeListener(_controllerListener);
            base.dispose();
        }

        private void _controllerListener() {
            var isTextEmpty = widget.controller.text.Length > 0;
            if (_isHintTextHidden != isTextEmpty)
                setState(() => { _isHintTextHidden = isTextEmpty; });
        }

        public override Widget build(BuildContext context) {
            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: new List<Widget> {
                    _buildLabelText(),
                    new Stack(
                        children: new List<Widget> {
                            _buildEditableText(), _buildHintText()
                        }
                    )
                }
            );
        }

        private Widget _buildLabelText() {
            if (widget.labelText == null) return new Container();
            if (!_isHintTextHidden) return new Container(height: 20);

            return new Container(
                height: 20,
                alignment: Alignment.bottomLeft,
                child: new Text(widget.labelText,
                    style: widget.labelStyle
                )
            );
        }

        private Widget _buildHintText() {
            if (widget.hintText == null || _isHintTextHidden) return new Container();
            return new Positioned(
                top: 0,
                left: 0,
                bottom: 0,
                child: new GestureDetector(
                    onTap: () => {
                        var focusNode = widget.focusNode ?? _focusNode;
                        FocusScope.of(context).requestFocus(focusNode);
                    },
                    child: new Container(
                        alignment: Alignment.center,
                        child: new Text(widget.hintText,
                            style: widget.hintStyle
                        )
                    )
                )
            );
        }

        private Widget _buildEditableText() {
            return new GestureDetector(
                onTap: () => {
                    var focusNode = widget.focusNode ?? _focusNode;
                    FocusScope.of(context).requestFocus(focusNode);
                },
                child: new Container(
                    height: widget.height,
                    alignment: Alignment.center,
                    child: new EditableText(
                        maxLines: widget.maxLines,
                        controller: widget.controller,
                        focusNode: widget.focusNode ?? _focusNode,
                        autofocus: widget.autofocus,
                        obscureText: widget.obscureText,
                        style: widget.style,
                        cursorColor: widget.cursorColor,
                        autocorrect: widget.autocorrect,
                        textInputAction: widget.textInputAction,
                        textAlign: widget.textAlign,
                        scrollPadding: widget.scrollPadding,
                        onChanged: text => {
                            var isTextEmpty = text.Length > 0;
                            if (_isHintTextHidden != isTextEmpty)
                                setState(() => { _isHintTextHidden = isTextEmpty; });
                            widget.onChanged(text);
                        },
                        onSubmitted: widget.onSubmitted
                    )
                )
            );
        }
    }
}