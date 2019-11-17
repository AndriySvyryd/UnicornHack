import React from 'react';
import { computed } from 'mobx';
import { observer } from 'mobx-react';
import { DialogData } from '../transport/DialogData';
import { Dialog } from './Dialog';
import { IGameContext } from './Game';

export const PostGameStatisticsDialog = observer((props: IPostGameStatisticsProps) => {
    const { data, context } = props;
    return <Dialog context={context} show={computed(() => data.postGameStatistics != null)} className="postGameStatistics"
        title={computed(() => "You died...")} modal={true}
    >
        <PostGameStatistics {...props} />
    </Dialog>;
});

const PostGameStatistics = observer(({ context, data }: IPostGameStatisticsProps) => {
    const statistics = data.postGameStatistics;
    if (statistics == null) {
        return <></>;
    }

    return <a className="postGameStatistics__newGame" href="/">Start a new game.</a>;
});

interface IPostGameStatisticsProps {
    data: DialogData;
    context: IGameContext;
}