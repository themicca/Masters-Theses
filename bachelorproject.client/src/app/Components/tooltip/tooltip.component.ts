import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-tooltip',
  templateUrl: './tooltip.component.html',
  styleUrl: './tooltip.component.css'
})
export class TooltipComponent {
  @Input() sampleNode!: HTMLElement;
  @Input() sampleEdge!: HTMLElement;
  @Input() directionalCheckbox!: HTMLElement;
  @Input() weightedCheckbox!: HTMLElement;
  @Input() tooltipCheckbox!: HTMLElement;

  public buttonDijkstra!: HTMLElement;
  public buttonKruskal!: HTMLElement;
  public buttonEdmondsKarp!: HTMLElement;
  public buttonFleury!: HTMLElement;
  public buttonHeldKarp!: HTMLElement;
  public buttonGreedyMatching!: HTMLElement;
  public buttonGreedyColoring!: HTMLElement;
  public buttonWelshPowell!: HTMLElement;

  private tooltipElement!: HTMLElement;

  private readonly maxWidth: number = 400;
  private readonly offset: number = 10;

  private tooltipListeners = new Map<HTMLElement, {
    enter: (event: MouseEvent) => void;
    leave: (event: MouseEvent) => void;
    move: (event: MouseEvent) => void;
    tooltip: string;
  }>();

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.initTooltips();
    });
  }

  private initTooltips(): void {
    this.tooltipElement = document.createElement('div');
    this.tooltipElement.className = 'graph-tooltip';
    this.tooltipElement.style.display = 'none';
    this.tooltipElement.style.position = 'fixed';
    this.tooltipElement.style.backgroundColor = '#fff';
    this.tooltipElement.style.color = '#333';
    this.tooltipElement.style.border = '1px solid #ccc';
    this.tooltipElement.style.borderRadius = '4px';
    this.tooltipElement.style.padding = '8px 12px';
    this.tooltipElement.style.fontSize = '14px';
    this.tooltipElement.style.fontFamily = 'Arial, sans-serif';
    this.tooltipElement.style.boxShadow = '0 2px 5px rgba(0, 0, 0, 0.15)';
    this.tooltipElement.style.zIndex = '999999';
    this.tooltipElement.style.maxWidth = `${this.maxWidth}px`;
    this.tooltipElement.style.whiteSpace = 'normal';
    this.tooltipElement.style.wordWrap = 'break-word';
    document.body.appendChild(this.tooltipElement);

    this.addElementTooltip(this.sampleNode, 'Drag this to the paper to make a node.');
    this.addElementTooltip(this.sampleEdge, 'Click this to create an edge.');
    this.addElementTooltip(this.directionalCheckbox, 'Switch between directed and undirected graph.');
    this.addElementTooltip(this.weightedCheckbox, 'Switch between weighted and unweighted graph.');
    this.addElementTooltip(this.tooltipCheckbox, 'Enable/Disable tooltips.');

    this.addElementTooltip(this.buttonDijkstra,
      `Finds the shortest path from the start node to all other connected nodes in the graph.
      Requires:
      - Start node
      - Non-negative weights on edges
      - (Optinal) Setting an end node finds the shortest path to that node
      - If end node is set, a path from start to end must exist

      The algorithm automatically discards all unreachable nodes, so the graph does not need to be connected.`
    );

    this.addElementTooltip(this.buttonKruskal,
      `Builds a Minimum Spanning Tree (MST) by adding the lowest-weight edges without forming cycles. If the graph is not connected, it finds the Minimum Spanning Forest (MSF).
      Requires:
      - Undirected graph
      - Weighted graph`
    );

    this.addElementTooltip(this.buttonEdmondsKarp,
      `Solves the maximum flow problem using the Ford-Fulkerson method with BFS.
      Requires:
      - Directed graph
      - Start (source) node
      - Target (sink) node
      - Non-negative edge capacities
      - A path from source to sink must exist.`
    );

    this.addElementTooltip(this.buttonFleury,
      `Finds an Eulerian path or circuit in a graph by carefully removing edges.
      Requires:
      - Undirected graph
      - Connected graph
      - Graph must be Eulerian or semi-Eulerian (0 or 2 odd-degree nodes)
      - Eulerian condition is checked before running the algorithm`
    );

    this.addElementTooltip(this.buttonHeldKarp,
      `Finds hamiltonian cycle using dynamic programming.
      Requires:
      - Start node
      - Non negative edge weights
      - Connected graph
      - Hamiltonian cycle must exist

      Assumes complete graph, if complete graph is not provided, it uses dijkstra to fill the edge matrix with missing weights.`
    );

    this.addElementTooltip(this.buttonGreedyMatching,
      `Finds a matching in a graph by repeatedly selecting the smallest available edge.
      Requires:
      - Undirected graph`
    );

    this.addElementTooltip(this.buttonGreedyColoring,
      `Colors nodes of a graph so that no two adjacent nodes have the same color.
      Requires:
      - Undirected graph`
    );

    this.addElementTooltip(this.buttonWelshPowell,
      `Optimizes node coloring by ordering vertices by degree.
      Requires:
      - Undirected graph`
    );
  }

  addElementTooltip(element: HTMLElement, tooltip: string) {
    const mouseEnter = () => {
      this.tooltipElement.innerText = tooltip;
      this.tooltipElement.style.display = 'block';
    }

    const mouseLeave = () => {
      this.tooltipElement.style.display = 'none';
    }

    const mouseMove = (event: MouseEvent) => {
      let posX = event.clientX + this.offset;
      let posY = event.clientY + this.offset;

      const tooltipWidth = this.tooltipElement.offsetWidth;
      const tooltipHeight = this.tooltipElement.offsetHeight;

      const overflowsRight = posX + this.maxWidth > window.innerWidth;
      const overflowsBottom = posY + tooltipHeight > window.innerHeight;

      if (overflowsRight && overflowsBottom) {
        posX = event.clientX - tooltipWidth;
        posY = event.clientY - tooltipHeight;
      } else if (overflowsRight) {
        posX = event.clientX - tooltipWidth;
      } else if (overflowsBottom) {
        posY = event.clientY - tooltipHeight;
      }

      this.tooltipElement.style.left = `${posX}px`;
      this.tooltipElement.style.top = `${posY}px`;
    }

    this.tooltipListeners.set(element, {
      enter: mouseEnter,
      leave: mouseLeave,
      move: mouseMove,
      tooltip: tooltip
    });

    element.addEventListener('mouseenter', mouseEnter);
    element.addEventListener('mouseleave', mouseLeave);
    element.addEventListener('mousemove', mouseMove);
  }

  public disableAllTooltips(): void {
    this.tooltipElement.style.display = 'none';
    for (const [element, handlers] of this.tooltipListeners.entries()) {
      element.removeEventListener('mouseenter', handlers.enter);
      element.removeEventListener('mouseleave', handlers.leave);
      element.removeEventListener('mousemove', handlers.move);
    }
  }

  public enableAllTooltips(): void {
    this.tooltipElement.style.display = 'block';
    for (const [element, data] of this.tooltipListeners.entries()) {
      this.addElementTooltip(element, data.tooltip);
    }
  }
}
