﻿# "Souls like" 多人游戏的网络设计
&emsp;&emsp;魂系列是典型的带多人游戏功能的单机游戏.在多人游戏中,服务器几乎完全不处理游戏逻辑,仅仅负责玩家匹配.
在Demo中,实现了类似的多人游戏机制,在实现部分有一些取巧,省了一些工作量,但使得高延迟时可能会出现一些不太合理的现象,
不过任何多人游戏,高延迟的体验一定都很差,设计时只要满足低延迟(<100ms)时表现正常即可.

## 魂系列多人游戏机制
&emsp;&emsp;在经过一些实际测试,以及网上查阅相关视频,发现魂系列的多人游戏有以下现象:

&emsp;&emsp;1.任何人都可以开修改器锁血和精力.这说明血量等状态完全是本地处理.

&emsp;&emsp;2.看到对方空气斩,但自己会掉血.或者跑开很远,结果被背刺.有时候已经开始处决对方,对方却仍然在行动.这说明攻击判定很有可能也是本地处理.

&emsp;&emsp;可以看出来,在多人游戏中,数据和相关逻辑基本都是本地处理,这也是单机游戏不可避免的问题,因为没有服务器做决断,在较高延迟或使用修改器时,很难做出合适的处理.

&emsp;&emsp;再考虑联机时的网络,有可能是完全点对点的网络设计,也可能存在主机,但主机仅仅负责不同玩家间的消息转发.

## 这里使用的机制
&emsp;&emsp;基于上面的观察结果,采用的机制如下:

&emsp;&emsp;1.所有玩家数据和状态均由本机处理.

&emsp;&emsp;2.采用主-从网络,主机负责转发不同玩家之间的消息.

## 同步的设计
&emsp;&emsp;有了上面的机制,还要确定不同机器之间如何做同步.在这里我把需要同步的内容分成两类:

&emsp;&emsp;1.__角色状态__.即角色当前的状态,这类同步可以采用不可靠协议,即使之前的内容丢包,只要收到最新的内容,就能确定角色当前状态,并立即显示出来.

&emsp;&emsp;2.__关键事件__.有部分事件可能比较关键,这类同步必须采用可靠协议,比如玩家射出一箭或者击中对方,在延迟不大时,不应该因为丢包而没有处理,而延迟过大时,则可以视情况不处理.

## 具体的实现

### 角色状态
&emsp;&emsp;首先是角色状态的同步,这里主要是角色的刚体运动,动画以及血量.实际可以根据情况附加更多状态信息
这里讲一下刚体运动和动画的同步.

#### 刚体运动的同步
&emsp;&emsp;采用的机制是"影子跟随".如果收到的位置与当前位置较近,则以一定速度移动到此为止.若距离较远,则直接"闪现"过去.
在此基础上,再加上刚体速度的同步,低延迟状态下效果几乎完美.

#### 动画的同步
&emsp;&emsp;这里使用了两个判断:

&emsp;&emsp;1.如果收到的动画与当前显示动画不同,则修改当前动画状态.

&emsp;&emsp;2.如果动画相同,但当前播放进度差异较大,则修改当前动画状态.

#### 角色状态同步的效果
首先看看低延迟情况(50ms ~ 100ms)
[//]: <> ( todo  低延迟GIF)

再看看较高延迟的情况(300ms ~ 2s)
[//]: <> ( todo  高延迟GIF)


### 关键事件
这里可以说的不多,比较关键的事件都可以这样处理,不同事件的具体表现可以根据情况优化.
比如伤害判定,本地玩家击中对方,则给对方发给消息,对方再根据自己的状态判定伤害是否有效.
另外我把手枪射击的消息也作为关键事件同步了,这样在较高延迟下,表现可能不太好.


## 总结
有了以上这些机制,已经可以实现魂系列的多人游戏了.

另外考虑下有服务器作判定的多人动作游戏,部分动作或技能会需要服务器判定是否使用,这样一来机制上会复杂一些,但总体思路是差不多的.