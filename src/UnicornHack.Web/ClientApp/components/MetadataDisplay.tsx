import * as React from 'React';
import * as htmlparser2 from 'htmlparser2';
import { TooltipTrigger } from './TooltipTrigger';

export function parseMetadata(input: string): React.ReactNode[] {
    const handler = new htmlparser2.DomHandler(null as any);
    new htmlparser2.Parser(handler).end(input);

    return processNodes((handler as any).dom);
}

function processNodes(nodes: htmlparser2.DomElement[] | undefined): React.ReactNode[] {
    if (nodes == undefined) {
        return [<></>];
    }

    return nodes
        .filter(node => node.type !== 'text' || node.data.trim() !== '')
        .map(nodeToComponent);
}

function nodeToComponent(node: htmlparser2.DomElement, index: number) {
    if (node.type == undefined) {
        throw "No type for node: " + node;
    }

    var handler = ElementHandlers.get(node.type);
    if (handler == undefined) {
        throw "No handler for node type: " + node.type;
    }

    return handler(node, index);
}

function handleTag(node: htmlparser2.DomElement, index: number) {
    switch (node.name) {
        case "damage":
            return <DamageTooltip key={index} index={index} {...node.attribs}>
                {processNodes(node.children)}
            </DamageTooltip>;
        default:
            throw "Unhandled tag type: " + node.name;
    }
}

const ElementHandlers: Map<string, (node: htmlparser2.DomElement, index: number) => React.ReactNode> =
    new Map<string, (node: htmlparser2.DomElement, index: number) => React.ReactNode>(
        [
            ["text", node => node.data],
            ["tag", handleTag],
            ["style", () => null],
            ["directive", () => null],
            ["comment", () => null],
            ["script", () => null],
            ["cdata", () => null],
            ["doctype", () => null]
        ]);

const DamageTooltip = (props: IDamageProps) =>
    <TooltipTrigger id={"damage-" + props.index} tooltip={formatDamage(props)}>
        <span className="annotatedText">{props.children}</span>
    </TooltipTrigger>;

function formatDamage(props: IDamageProps): React.ReactNode {
    var result = '';
    if (props.physical != undefined) {
        result += ' physical: ' + props.physical;
    }

    if (props.fire != undefined) {
        result += ' fire: ' + props.fire;
    }

    if (props.bleeding != undefined) {
        result += ' bleeding: ' + props.bleeding;
    }

    if (props.toxin != undefined) {
        result += ' toxin: ' + props.toxin;
    }

    if (props.acid != undefined) {
        result += ' acid: ' + props.acid;
    }

    if (props.cold != undefined) {
        result += ' cold: ' + props.cold;
    }

    if (props.light != undefined) {
        result += ' light: ' + props.light;
    }

    if (props.psychic != undefined) {
        result += ' psychic: ' + props.psychic;
    }

    if (props.electricity != undefined) {
        result += ' electricity: ' + props.electricity;
    }

    if (props.water != undefined) {
        result += ' water: ' + props.water;
    }

    if (props.sonic != undefined) {
        result += ' sonic: ' + props.sonic;
    }

    if (props.void != undefined) {
        result += ' void: ' + props.void;
    }

    if (props.drain != undefined) {
        result += ' life drain: ' + props.drain;
    }

    return result.trim();
}

interface IDamageProps extends React.ComponentPropsWithoutRef<any> {
    index: number;
    physical?: string;
    fire?: string;
    bleeding?: string;
    toxin?: string;
    acid?: string;
    cold?: string;
    light?: string;
    psychic?: string;
    electricity?: string;
    water?: string;
    sonic?: string;
    void?: string;
    drain?: string;
}