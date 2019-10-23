using System.Collections.Generic;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using ConnectApp.Utils;
using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class AtAllMention {
        
    }
    
    public class ChannelMentionScreenConnector : StatelessWidget {
        public ChannelMentionScreenConnector(
            string channelId,
            Key key = null
        ) : base(key: key) {
            this.channelId = channelId;
        }

        readonly string channelId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, ChannelMentionScreenViewModel>(
                converter: state => new ChannelMentionScreenViewModel {
                    channel = state.channelState.channelDict[key: this.channelId],
                    mentionSuggestions = state.channelState.mentionSuggestions.getOrDefault(key: this.channelId, null),
                    userDict = state.userState.userDict,
                    mentionLoading = state.channelState.mentionLoading
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new ChannelMentionScreenActionModel {
                        mainRouterPop = () => dispatcher.dispatch(new MainNavigatorPopAction()),
                        chooseMentionCancel = () => {
                            dispatcher.dispatch(new ChannelChooseMentionCancelAction());
                            dispatcher.dispatch(new MainNavigatorPopAction());
                        },
                        chooseMentionConfirm = (mentionUserId, mentionUserName) => {
                            dispatcher.dispatch(new ChannelChooseMentionConfirmAction {
                                mentionUserId = mentionUserId,
                                mentionUserName = mentionUserName
                            });
                            dispatcher.dispatch(new MainNavigatorPopAction());
                        },
                        startLoadingMention = () => {
                            dispatcher.dispatch(new StartFetchChannelMentionSuggestionAction());
                            dispatcher.dispatch<IPromise>(
                                Actions.fetchChannelMentionSuggestions(channelId: this.channelId));
                        }
                    };
                    return new ChannelMentionScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class ChannelMentionScreen : StatefulWidget {
        public ChannelMentionScreen(
            ChannelMentionScreenViewModel viewModel = null,
            ChannelMentionScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly ChannelMentionScreenViewModel viewModel;
        public readonly ChannelMentionScreenActionModel actionModel;

        public override State createState() {
            return new _ChannelMentionScreenState();
        }
    }

    class _ChannelMentionScreenState : State<ChannelMentionScreen>, RouteAware {
        readonly TextEditingController _editingController = new TextEditingController();
        readonly ScrollController _scrollController = new ScrollController();
        readonly List<object> mentionList = new List<object>();

        string curQuery = "";

        public override void initState() {
            if (this.widget.viewModel.mentionSuggestions == null) {
                SchedulerBinding.instance.addPostFrameCallback(_ => { this.widget.actionModel.startLoadingMention(); });
            }

            this.updateMentionList();
            base.initState();
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Router.routeObserve.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Router.routeObserve.unsubscribe(this);
            base.dispose();
        }

        void updateMentionList() {
            this.mentionList.Clear();
            var allMentions =
                this.widget.viewModel.mentionSuggestions ??
                this.widget.viewModel.channel.membersDict;

            foreach (var memberKey in allMentions.Keys) {
                var member = allMentions[key: memberKey];
                if (this.curQuery == "" || member.user.fullName.ToLower().Contains(this.curQuery.ToLower())) {
                    this.mentionList.Add(item: member);
                }
            }

            if (this.curQuery == "") {
                var myMemberInfo = this.widget.viewModel.channel.currentMember;
                if (myMemberInfo.role == "admin" ||
                    myMemberInfo.role == "owner" ||
                    myMemberInfo.role == "moderator") {
                    this.mentionList.Insert(0, new AtAllMention());
                }
            }
        }

        public override Widget build(BuildContext context) {
            this.updateMentionList();
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Container(
                        color: CColors.Background,
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(),
                                this._buildSearchBar(),
                                new Expanded(
                                    child: this.widget.viewModel.mentionLoading ? 
                                        new GlobalLoading() : this._buildMentionList()
                                )
                            }
                        )
                    )
                )
            );
        }

        void _onSearch(string query) {
            this.curQuery = query;
            this.setState(() => {
                if (this._scrollController.hasClients) {
                    this._scrollController.jumpTo(0);
                }
                this.updateMentionList();
            });
        }

        Widget _buildMentionList() {
            if (this.mentionList.Count == 0) {
                return new Container(
                    color: CColors.Background
                );
            }
            return new Container(
                color: CColors.Background,
                child: new CustomScrollbar(
                    ListView.builder(
                        physics: new AlwaysScrollableScrollPhysics(),
                        controller: this._scrollController,
                        itemCount: this.mentionList.Count,
                        itemBuilder: this._buildMentionTile
                    )
                )
            );
        }

        Widget _buildMentionTile(BuildContext context, int index) {
            var mentionObj = this.mentionList[index: index];

            if (mentionObj is ChannelMember) {
                ChannelMember member = (ChannelMember)this.mentionList[index: index];
                if (!this.widget.viewModel.userDict.ContainsKey(key: member.user.id)) {
                    return new Container();
                }

                var user = this.widget.viewModel.userDict[key: member.user.id];
                return new GestureDetector(
                    onTap: () => this.widget.actionModel.chooseMentionConfirm(member.user.id, member.user.fullName),
                    child: new Container(
                        color: CColors.White,
                        height: 60,
                        child: new Row(
                            children: new List<Widget> {
                                new SizedBox(width: 16),
                                Avatar.User(user: user, 32),
                                new Expanded(
                                    child: new Container(
                                        padding: EdgeInsets.symmetric(0, 16),
                                        child: new Column(
                                            mainAxisAlignment: MainAxisAlignment.center,
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: new List<Widget> {
                                                new Flexible(child: new Text(
                                                    data: user.fullName,
                                                    style: CTextStyle.PLargeBody,
                                                    maxLines: 1,
                                                    overflow: TextOverflow.ellipsis
                                                ))
                                            }
                                        )
                                    )
                                )
                            }
                        )
                    )
                );
            }
            
            if (mentionObj is AtAllMention) {
                return new GestureDetector(
                    onTap: () => this.widget.actionModel.chooseMentionConfirm("everyone", "所有人"),
                    child: new Container(
                        color: CColors.White,
                        height: 60,
                        child: new Row(
                            children: new List<Widget> {
                                new Expanded(
                                    child: new Container(
                                        padding: EdgeInsets.symmetric(0, 16),
                                        child: new Column(
                                            mainAxisAlignment: MainAxisAlignment.center,
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: new List<Widget> {
                                                new Flexible(child: new Text(
                                                    data: "@所有人",
                                                    style: CTextStyle.PLargeBody,
                                                    maxLines: 1,
                                                    overflow: TextOverflow.ellipsis
                                                ))
                                            }
                                        )
                                    )
                                )
                            }
                        )
                    )
                );
            }

            return new Container();
        }

        Widget _buildSearchBar() {
            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(16, 16, 16, 12),
                child: new InputField(
                    decoration: new BoxDecoration(
                        color: CColors.Separator2,
                        borderRadius: BorderRadius.all(8)
                    ),
                    height: 36,
                    controller: this._editingController,
                    style: CTextStyle.PLargeBody2,
                    prefix: new Container(
                        padding: EdgeInsets.only(8, right: 4),
                        child: new Icon(
                            icon: Icons.search,
                            size: 24,
                            color: CColors.BrownGrey
                        )
                    ),
                    hintText: "搜索",
                    hintStyle: CTextStyle.PLargeBody4.merge(new TextStyle(height: 1)),
                    cursorColor: CColors.PrimaryBlue,
                    textInputAction: TextInputAction.search,
                    clearButtonMode: InputFieldClearButtonMode.whileEditing,
                    onChanged: this._onSearch,
                    onSubmitted: this._onSearch
                )
            );
        }

        Widget _buildNavigationBar() {
            return new Container(
                decoration: new BoxDecoration(
                    color: CColors.White
                ),
                height: 44,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(width: 64),
                        new Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: new List<Widget> {
                                new Text(
                                    "选择提醒的人",
                                    style: CTextStyle.PXLargeMedium
                                ),
                                new SizedBox(height: 5),
                                new Text(
                                    "按活跃度排序",
                                    style: new TextStyle(
                                        fontSize: 10,
                                        fontFamily: "Roboto-Regular",
                                        color: CColors.Black
                                    )
                                )
                            }),
                        new CustomButton(
                            padding: EdgeInsets.symmetric(10, 16),
                            onPressed: () => this.widget.actionModel.chooseMentionCancel(),
                            child: new Text(
                                "取消",
                                style: CTextStyle.PLargeBlue
                            )
                        )
                    }
                )
            );
        }

        public void didPopNext() {
            StatusBarManager.statusBarStyle(false);
        }

        public void didPush() {
        }

        public void didPop() {
        }

        public void didPushNext() {
        }
    }
}