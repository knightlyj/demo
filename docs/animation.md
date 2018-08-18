# "Souls like"动画系统设计
&emsp;&emsp;与其说是魂系列动画系统,不如说是第三人称ARPG通用动画系统,设计上并不难,动画系统实现上下半身分离即可,再加上一些IK就很完美了.
此文主要描述Unity中实现这样动画系统中的经验,所用Unity版本为5.4.2.xxx.

## 整体设计
&emsp;&emsp;在黑魂中,有很多动作是上下半身无关的,比如可以一边走路一边格档或者换武器,而有的动作又是上下半身必须同时做,比如攻击动作.
所以在最初的设计中,我给各个动作进行了分类,有上半身动作,下半身动作,全身动作.

&emsp;&emsp;Unity中实现此功能的方法为Animator Layers,按照设想,有3个Layers,分别是UpperBody,LowerBody,WholeBody,
如果是全身动作,就切换到WholeBody Layer来表现.而半身分离的动作,就用UpperBody和LowerBody Layers来叠加表现.
一直采用这个设计完成了不少功能,后续遇到一些问题(主要是Layer的Blending模式问题,问题原因还没弄清楚,不在此描述了),遂放弃了这个方案,
只有UpperBody和LowerBody两层,不再在部分动作时切换到全身层.而这其中也遇到一点问题.

### 动画事件的问题
&emsp;&emsp;作为动作游戏,必然要用到动画事件,而动画事件是依附于Animation Clip,上下半身播放同一个Clip时,会触发两次事件.
如果不做处理会导致不少问题,比如跳跃时,可能施加了两次力(那么力度减半就好了- -).

&emsp;&emsp;想要解决这个问题,最直接的办法应该是知道事件来源于哪一个Animator Layer,但比较坑的是,在这个Unity的版本中,没有找到办法知道事件来源的Layer.
这样一来,要想其他办法解决.通过搜集资料以及个人思考,目前较可靠的方法有两种.
一是拷贝一份不带任何事件的Animation Clip,这样只有一层会触发事件了.
二是通过事件触发时间来判断,如果同一个事件几乎同时触发了两次,那么第二次事件可以放弃不处理.

&emsp;&emsp;在我的实现中,使用了第二个方法,如果一个动画事件多次触发会影响功能,则加上时间判断.


## 脚部IK
&emsp;&emsp;魂系列游戏中,站立时脚部的位置和角度会根据地面调整,首先看一个魂2老王的GIF,注意老王的左脚

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/ds-badIK.gif)

&emsp;&emsp;在看看角色的GIF

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/ds-roleIK.gif)

&emsp;&emsp;可以看出来,大概原理是如果动画中的脚部有插入地面,则将脚部放置到地面位置.如果脚部悬空,则没有处理.
实现方法就很简单了,根据当前动画的脚部位置,从上方一定距离向下发射射线,即可知道脚部是否插入了地面.
如果射线碰到了地面,将脚部放置于这个位置,另外得到接触点的法线,求方向导数可以知道脚掌的倾斜角度.

在Demo中,仅处理了角色idle时的IK,效果图如下.

![](https://raw.githubusercontent.com/knightlyj/demo/master/docs/img/demo-IK.gif)

可以看到角色Idle时,脚部位于地面,且脚掌贴合地面.

## 总结
实现了以上的功能,基本可以完成黑魂的全部动作.得益于Unity强大的动画功能,完成这样的系统设计并不是太难.
