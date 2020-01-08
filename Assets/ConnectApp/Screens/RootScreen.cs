using System.Collections.Generic;
using ConnectApp.Components;
using ConnectApp.Constants;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.screens {
    public class RootScreen : StatelessWidget {
        public override Widget build(BuildContext context) {
            if (Application.platform == RuntimePlatform.Android) {
                return new Container(
                    color: CColors.White);
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.end,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget> {
                            new Container(
                                width: 182,
                                height: 32,
                                child: Image.asset("image/iOS/unityConnectBlack.imageset/unityConnectBlack",
                                    fit: BoxFit.cover)
                            ),
                            new Container(
                                width: 101,
                                height: 22,
                                margin: EdgeInsets.only(bottom: 16, top: 16),
                                child: Image.asset("image/iOS/madeWithUnity.imageset/madeWithUnity", fit: BoxFit.cover))
                        })
                ));
        }
    }
}