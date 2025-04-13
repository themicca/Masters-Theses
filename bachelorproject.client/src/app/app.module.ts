import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { GraphSubmitComponent } from './Components/graph-submit/graph-submit.component';
import { FormsModule } from '@angular/forms';
import { GraphEditorComponent } from './Components/graph-editor/graph-editor.component';
import { GraphConstructFromBackendComponent } from './Components/graph-construct-from-backend/graph-construct-from-backend.component';
import { GraphSelectionComponent } from './Components/graph-selection/graph-selection.component';
import { EdgeWeightComponent } from './Components/edge-weight/edge-weight.component';
import { EdgesConnectionComponent } from './Components/edges-connection/edges-connection.component';
import { RenameNodesComponent } from './Components/rename-nodes/rename-nodes.component';
import { TooltipComponent } from './Components/tooltip/tooltip.component';
import { ErrorPopupComponent } from './Components/error-popup/error-popup.component';

@NgModule({
  declarations: [
    AppComponent,
    GraphSubmitComponent,
    GraphEditorComponent,
    GraphConstructFromBackendComponent,
    GraphSelectionComponent,
    EdgeWeightComponent,
    EdgesConnectionComponent,
    RenameNodesComponent,
    TooltipComponent,
    ErrorPopupComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
