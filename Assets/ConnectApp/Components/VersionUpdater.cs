using ConnectApp.Api;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using ConnectApp.Utils;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class VersionUpdater : StatefulWidget {
        public VersionUpdater(
            Widget child = null,
            Key key = null
        ) : base(key: key) {
            this.child = child;
        }

        public readonly Widget child;

        public override State createState() {
            return new _VersionUpdaterState();
        }
    }

    public class _VersionUpdaterState : State<VersionUpdater> {
        public override void initState() {
            base.initState();
            fetchInitData();
            VersionManager.checkForUpdates(type: CheckVersionType.initialize);
            if (UserInfoManager.isLogin()) {
                var userId = UserInfoManager.initUserInfo().userId ?? "";
                if (userId.isNotEmpty()) {
                    StoreProvider.store.dispatcher.dispatch(Actions.fetchUserProfile(userId: userId));
                }
            }
        }

        static void fetchInitData() {
            LoginApi.InitData().Then(initDataResponse => {
                if (initDataResponse.VS.isNotEmpty()) {
                    HttpManager.updateCookie($"VS={initDataResponse.VS}");
                }
            }).Catch(onRejected: Debuger.LogError);
        }

        public override Widget build(BuildContext context) {
            return this.widget.child;
        }
    }
}