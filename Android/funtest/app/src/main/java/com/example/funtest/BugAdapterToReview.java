package com.example.funtest;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.funtest.objects.Bug;

import java.util.ArrayList;

public class BugAdapterToReview extends RecyclerView.Adapter<BugAdapterToReview.MyViewHolder> {

    private Context mContext;
    private ArrayList<Bug> bugList;


    public BugAdapterToReview(Context context, ArrayList<Bug> bugList) {
        this.mContext = context;
        this.bugList = bugList;
    }

    //methods regarding extending recyvlerview Adapter
    @NonNull
    @Override
    public MyViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(mContext).inflate(R.layout.bug_list_item_toreview,parent,false);
        return new MyViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull BugAdapterToReview.MyViewHolder holder, @SuppressLint("RecyclerView") int position) {
        holder.bugName.setText(bugList.get(position).getName());
        holder.bugState.setText(bugList.get(position).getState());
        String retestsRequired = String.valueOf(bugList.get(position).getRetestsRequired());
        String retestsDone = String.valueOf(bugList.get(position).getRetestsDone());
        String retestsFailed = String.valueOf(bugList.get(position).getRetestsFailed());
        holder.bugRetestsRequired.setText(retestsRequired);
        holder.bugRetestsDone.setText(retestsDone);
        holder.bugRetestsFailed.setText(retestsFailed);


        holder.itemView.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(mContext, BugDetailsActivityToReview.class);
                intent.putExtra("position",position);
                mContext.startActivity(intent);
            }
        });

    }

    @Override
    public int getItemCount() {
        return this.bugList.size();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    //MyViewHolder that BugAdapter uses
    public class MyViewHolder extends RecyclerView.ViewHolder {

        TextView bugName;
        TextView bugState;
        TextView bugRetestsRequired;
        TextView bugRetestsDone;
        TextView bugRetestsFailed;


        public MyViewHolder(@NonNull View itemView) {
            super(itemView);
            bugName = itemView.findViewById(R.id.bil_textView_name);
            bugState = itemView.findViewById(R.id.bil_textView_state);
            bugRetestsRequired =  (TextView) itemView.findViewById(R.id.bil_textView_retestsRequired);
            bugRetestsDone =  (TextView) itemView.findViewById(R.id.bil_textView_retestsDone);
            bugRetestsFailed =  (TextView) itemView.findViewById(R.id.bil_textView_retestsFailed);

        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////
}
